/**
 * Tetrino, a .NET wrapper and plug-in for Rhinoceros 3d and Grasshopper
 * for Tetgen (http://tetgen.org).
 *
 * Copyright (C) 2017 Tom Svilans (http://tomsvilans.com)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using TetgenSharp;
using Grasshopper.Kernel.Data;
using System.Linq;
using Grasshopper.Kernel.Types;

namespace TetgenGH
{
    public class Tetgen_Component : GH_Component
    {
        public Tetgen_Component()
          : base("Tetrahedralize", "Tetgen",
              "Tetrahedralizes the mesh using Tetgen library.",
              "Mesh", "Triangulation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to tetrahedralize.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Flags", "F", "Tetgen flags. 0 = return list of tetrahedra, 1 = return triangulated mesh, 2 = return tetra indices, "
                + "3 = return edge indices.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("MinRatio", "R", "Tetrahedron ratio.", GH_ParamAccess.item, 2.0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Output mesh.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Indices", "I", "Output indices. If F == 2, then this will be a tree with tetrahedra indices as sub-lists (4 per list)." + 
                " If F == 3, then this will be a tree with edge indices as sub-lists (2 per list).", GH_ParamAccess.tree);
            pManager.AddPointParameter("Points", "P", "Output points. If F is 2 or 3, then the indices from I will correspond to this point list.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            if (!DA.GetData("Mesh", ref mesh))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No input mesh specified.");
                return;
            }

            int flags = 1;
            DA.GetData("Flags", ref flags);

            double minratio = 2.0;
            DA.GetData("MinRatio", ref minratio);
            if (minratio <= 1.0) minratio = 1.1;

            TetgenSharp.TetgenBehaviour b = new TetgenSharp.TetgenBehaviour();
            b.quality = 1;
            b.plc = 1;
            b.minratio = minratio;
            b.coarsen = 1;

            TetgenMesh tin = TetgenRC.ExtensionMethods.ToTetgenMesh(mesh);
            TetgenSharp.TetgenMesh tm = TetgenSharp.TetRhino.Tetrahedralize(tin, b);

            DataTree<int> indices;
            int N, index = 0;
            GH_Path path;

            switch (flags)
            {
                case (0):
                    DA.SetDataList("Mesh", TetgenRC.ExtensionMethods.TetraToRhinoMesh(tm).Select(x => new GH_Mesh(x)));
                    break;
                case (1):
                    DA.SetDataList("Mesh", new GH_Mesh[] { new GH_Mesh(TetgenRC.ExtensionMethods.ToRhinoMesh(tm)) });
                    break;
                case (2):
                    indices = new DataTree<int>();
                    N = tm.TetraIndices.Length / 4;
                    for (int i = 0; i < N; ++i)
                    {
                        path = new GH_Path(i);
                        index = i * 4;

                        indices.Add(tm.TetraIndices[index], path);
                        indices.Add(tm.TetraIndices[index + 1], path);
                        indices.Add(tm.TetraIndices[index + 2], path);
                        indices.Add(tm.TetraIndices[index + 3], path);
                    }

                    DA.SetDataTree(1, indices);
                    DA.SetDataList("Points", TetgenRC.ExtensionMethods.ToPointList(tm));
                    break;
                case (3):
                    indices = new DataTree<int>();
                    N = tm.EdgeIndices.Length / 2;
                    for (int i = 0; i < N; ++i)
                    {
                        path = new GH_Path(i);
                        index = i * 2;

                        indices.Add(tm.EdgeIndices[index], path);
                        indices.Add(tm.EdgeIndices[index + 1], path);
                    }

                    DA.SetDataTree(1, indices);
                    DA.SetDataList("Points", TetgenRC.ExtensionMethods.ToPointList(tm));
                    break;
                default:
                    break;
            }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.TetgenGH_icon;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d8e38d8a-5660-409d-9687-d0b90ef2a7be"); }
        }
    }
}
