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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using TetgenSharp;

namespace TetgenRC
{
    /// <summary>
    /// TetgenRC extension methods for existing Rhino types
    /// </summary>
    public static class ExtensionMethods
    {
        public static TetgenMesh ToTetgenMesh(this Mesh m)
        {
            Mesh M = m.DuplicateMesh();
            //M.Faces.ConvertQuadsToTriangles();
            M.UnifyNormals();

            TetgenMesh tm = new TetgenMesh();
            tm.Vertices = new double[M.Vertices.Count * 3];

            var FaceIndexCount = 0;
            tm.FaceSizes = new int[M.Faces.Count];

            for (int i = 0; i < M.Faces.Count; ++i)
            {
                int fsize = M.Faces[i].IsQuad ? 4 : 3;
                FaceIndexCount += fsize;
                tm.FaceSizes[i] = fsize;
            }

            tm.FaceIndices = new int[FaceIndexCount];

            for (int i = 0; i < M.Vertices.Count; ++i)
            {
                tm.Vertices[i * 3] = M.Vertices[i].X;
                tm.Vertices[i * 3 + 1] = M.Vertices[i].Y;
                tm.Vertices[i * 3 + 2] = M.Vertices[i].Z;
            }

            int fi = 0;
            for (int i = 0; i < M.Faces.Count; ++i)
            {
                tm.FaceIndices[fi] = M.Faces[i].A; fi++;
                tm.FaceIndices[fi] = M.Faces[i].B; fi++;
                tm.FaceIndices[fi] = M.Faces[i].C; fi++;
                if (M.Faces[i].IsQuad)
                    tm.FaceIndices[fi] = M.Faces[i].D; fi++;
            }

            return tm;
        }

        public static List<Point3d> ToPointList(this TetgenMesh tm)
        {
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < tm.Vertices.Length; i += 3)
            {
                points.Add(new Point3d(
                  tm.Vertices[i],
                  tm.Vertices[i + 1],
                  tm.Vertices[i + 2]
                  ));
            }

            return points;
        }

        public static Mesh ToRhinoMesh(this TetgenMesh tm)
        {
            Mesh m = new Mesh();


            for (int i = 0; i < tm.Vertices.Length; i += 3)
            {
                m.Vertices.Add(
                    tm.Vertices[i],
                    tm.Vertices[i + 1],
                    tm.Vertices[i + 2]
                    );
            }

            for (int i = 0; i < tm.FaceIndices.Length; i += 3)
            {
                m.Faces.AddFace(
                  tm.FaceIndices[i],
                  tm.FaceIndices[i + 1],
                  tm.FaceIndices[i + 2]
                  );
            }

            m.Vertices.CullUnused();
            
            m.FaceNormals.ComputeFaceNormals();
            m.Normals.ComputeNormals();
            m.UnifyNormals();

            if (m.SolidOrientation() <= 0)
                m.Flip(true, true, true);

            return m;
        }

        public static Mesh[] TetraToRhinoMesh(this TetgenMesh tm)
        {
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < tm.Vertices.Length; i += 3)
            {
                points.Add(new Point3d(
                  tm.Vertices[i],
                  tm.Vertices[i + 1],
                  tm.Vertices[i + 2]
                  ));
            }

            Mesh[] tetra = new Mesh[tm.TetraIndices.Length / 4];

            for (int i = 0; i < tetra.Length; ++i)
            {
                int ivert = i * 4;
                Mesh t = new Mesh();
                t.Vertices.Add(points[tm.TetraIndices[ivert]]);
                t.Vertices.Add(points[tm.TetraIndices[ivert + 1]]);
                t.Vertices.Add(points[tm.TetraIndices[ivert + 2]]);
                t.Vertices.Add(points[tm.TetraIndices[ivert + 3]]);

                t.Faces.AddFace(0, 1, 2);
                t.Faces.AddFace(1, 2, 3);
                t.Faces.AddFace(2, 3, 0);
                t.Faces.AddFace(3, 0, 1);

                t.FaceNormals.ComputeFaceNormals();
                t.Normals.ComputeNormals();
                t.UnifyNormals();

                tetra[i] = t;
            }
            return tetra;
        }

    }
}
