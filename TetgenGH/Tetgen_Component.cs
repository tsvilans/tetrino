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

        private int MeshOutIndex, IndicesOutIndex, PointsOutIndex, FacesOutIndex;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to tetrahedralize.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Flags", "F", "Tetgen flags. 0 = return list of tetrahedra, 1 = return triangulated mesh, 2 = return tetra indices, "
                + "3 = return edge indices.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("MinRatio", "R", "Tetrahedron ratio.", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("MaxVolume", "MV", "Tetrahedron max volume.", GH_ParamAccess.item, -1);
            pManager.AddIntegerParameter("Steiner", "S", "Steiner mode.", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Coarsen", "C", "Coarsen.", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            MeshOutIndex = pManager.AddMeshParameter("Mesh", "M", "Output mesh.", GH_ParamAccess.tree);
            IndicesOutIndex = pManager.AddIntegerParameter("Indices", "I", "Output indices. If F == 2, then this will be a tree with tetrahedra indices as sub-lists (4 per list)." + 
                " If F == 3, then this will be a tree with edge indices as sub-lists (2 per list).", GH_ParamAccess.tree);
            PointsOutIndex = pManager.AddPointParameter("Points", "P", "Output points. If F is 2 or 3, then the indices from I will correspond to this point list.", GH_ParamAccess.tree);
            FacesOutIndex = pManager.AddIntegerParameter("Face indices", "FI", "Output indices for faces.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> meshes = new List<Mesh>();

            if (!DA.GetDataList("Mesh", meshes))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No input mesh specified.");
                return;
            }

            int flags = 1;
            DA.GetData("Flags", ref flags);

            double minratio = 2.0;
            DA.GetData("MinRatio", ref minratio);
            if (minratio <= 1.0) minratio = 1.1;

            double maxvolume = 2.0;
            DA.GetData("MaxVolume", ref maxvolume);

            int steiner = 0;
            DA.GetData("Steiner", ref steiner);

            int coarsen = 0;
            DA.GetData("Coarsen", ref coarsen);

            DataTree<int> indices = new DataTree<int>();
            DataTree<GH_Mesh> meshes_out = new DataTree<GH_Mesh>();
            DataTree<GH_Point> points_out = new DataTree<GH_Point>();

            var face_indices = new DataTree<int>();
            var verts_indices = new DataTree<int>();

            int N, index = 0;
            GH_Path path;

            for (int i = 0; i < meshes.Count; ++i)
            {
                if (maxvolume > 0)
                {
                    var vmp = VolumeMassProperties.Compute(meshes[i]);
                    maxvolume = Math.Max(maxvolume, vmp.Volume / 100000); // Safety so as not to end up with too many elements...
                }

                TetgenSharp.TetgenBehaviour b = new TetgenSharp.TetgenBehaviour();
                b.quality = 1;
                b.plc = 1;
                b.minratio = minratio;
                b.coarsen = coarsen;
                b.maxvolume = maxvolume;
                b.supsteiner_level = steiner;

                TetgenMesh tin = TetgenRC.ExtensionMethods.ToTetgenMesh(meshes[i]);
                TetgenSharp.TetgenMesh tm = TetgenSharp.TetRhino.Tetrahedralize(tin, b);
                //path = new GH_Path(i);

                if (tm == null)
                {
                    this.Message = "Failed.";
                    return;
                }

                switch (flags)
                {
                    case (0):
                        meshes_out.AddRange(TetgenRC.ExtensionMethods.TetraToRhinoMesh(tm).Select(x => new GH_Mesh(x)), new GH_Path(i, 0));
                        break;
                    case (1):
                        meshes_out.Add(new GH_Mesh(TetgenRC.ExtensionMethods.ToRhinoMesh(tm)), new GH_Path(i, 0));
                        break;
                    case (2):

                        N = tm.TetraIndices.Length / 4;
                        for (int j = 0; j < N; ++j)
                        {
                            path = new GH_Path(i, j);
                            index = j * 4;

                            indices.Add(tm.TetraIndices[index], path);
                            indices.Add(tm.TetraIndices[index + 1], path);
                            indices.Add(tm.TetraIndices[index + 2], path);
                            indices.Add(tm.TetraIndices[index + 3], path);
                        }

                        N = tm.FaceSizes.Length;

                        int fi = 0;
                        for (int j = 0; j < N; ++j)
                        {
                            var fpath = new GH_Path(i, j);
                            for (int k = 0; k < tm.FaceSizes[j]; ++k)
                            {
                                face_indices.Add(tm.FaceIndices[fi], fpath);
                                fi++;
                            }
                        }

                        points_out.AddRange(TetgenRC.ExtensionMethods.ToPointList(tm).Select(x => new GH_Point(x)), new GH_Path(i, 0));
                        break;
                    case (3):
                        N = tm.EdgeIndices.Length / 2;
                        for (int j = 0; j < N; ++j)
                        {
                            path = new GH_Path(i, j);
                            index = j * 2;

                            indices.Add(tm.EdgeIndices[index], path);
                            indices.Add(tm.EdgeIndices[index + 1], path);
                        }

                        points_out.AddRange(TetgenRC.ExtensionMethods.ToPointList(tm).Select(x => new GH_Point(x)), new GH_Path(i, 0));
                        break;
                    default:
                        break;
                }
            }

            DA.SetDataTree(MeshOutIndex, meshes_out);
            DA.SetDataTree(IndicesOutIndex, indices);
            DA.SetDataTree(PointsOutIndex, points_out);
            DA.SetDataTree(FacesOutIndex, face_indices);
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
