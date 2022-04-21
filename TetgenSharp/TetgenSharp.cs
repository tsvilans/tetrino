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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TetgenSharp
{
    /// <summary>
    /// Class to hold return data from Tetgen.
    /// </summary>
    public class TetgenMesh
    {
        public double[] Vertices;
        public int[] FaceIndices;
        public int[] FaceSizes;
        public int[] TetraIndices;
        public int[] EdgeIndices;

        /// <summary>
        /// Duplicate TetgenMesh.
        /// </summary>
        /// <returns></returns>
        public TetgenMesh Duplicate()
        {
            TetgenMesh cm = new TetgenMesh();
            cm.Vertices = new double[Vertices.Length];
            cm.FaceIndices = new int[FaceIndices.Length];
            cm.FaceSizes = new int[FaceSizes.Length];
            cm.TetraIndices = new int[TetraIndices.Length];
            cm.EdgeIndices = new int[EdgeIndices.Length];

            Vertices.CopyTo(cm.Vertices, 0);
            FaceIndices.CopyTo(cm.FaceIndices, 0);
            FaceSizes.CopyTo(cm.FaceSizes, 0);
            TetraIndices.CopyTo(cm.TetraIndices, 0);
            EdgeIndices.CopyTo(cm.EdgeIndices, 0);
            return cm;
        }
    }

    public class TetgenBehaviour
    {
        public int plc = 0;
        public int refine = 0;
        public int quality = 0;
        public int coarsen = 0;
        public int insertaddpoints = 0;

        public int supsteiner_level = 2;
        public int optlevel = 2;

        public double maxvolume = -1.0;
        public double minratio = 2.0;
        public double mindihedral = 0.0;

        public double optmaxdihedral = 165.00;
        public double optminslidihed = 179.00;
        public double optminsmtdihed = 179.00;
    }

    /// <summary>
    /// Contains the methods for performing CSG operations using Carve
    /// </summary>
    public static class TetRhino
    {
        /// <summary>
        /// Defines the low-level structure that the DLL wrapper uses
        /// to represent a triangular mesh.
        /// </summary>
        private unsafe struct InteropMesh
        {
            /// <summary>
            /// The array containing the vertices
            /// </summary>
            public double* vertices;

            /// <summary>
            /// The array containing the triangle indices
            /// </summary>
            public int* faceIndices;

            /// <summary>
            /// The array containing the face sizes (number of verts
            /// per face)
            /// </summary>
            public int* faceSizes;

            /// <summary>
            /// The array containing the tetra indices
            /// </summary>
            public int* tetra;

            public int* edges;

            /// <summary>
            /// The number of elements in the vertices array
            /// </summary>
            public int numVertices;

            /// <summary>
            /// The number of faces in the mesh. 
            /// </summary>
            public int numFaces;

            /// <summary>
            /// The number of indices in the face indices array
            /// </summary>
            public int numFaceIndices;

            /// <summary>
            /// The number of tetra
            /// </summary>
            public int numTetra;

            public int numEdges;
        }

        private unsafe struct TetgenBehaviourUnsafe
        {
            public int plc;
            public int refine;
            public int quality;
            public int coarsen;
            public int insertaddpoints;

            public int supsteiner_level;
            public int optlevel;

            public double maxvolume;
            public double minratio;
            public double mindihedral;

            public double optmaxdihedral;
            public double optminslidihed;
            public double optminsmtdihed;

        };

        /// <summary>
        /// The DLL entry definition for performing CSG operations.
        /// </summary>
        /// <param name="a">The first mesh</param>
        [DllImport("TetgenWrapper.dll")]
        private static unsafe extern InteropMesh* performTetgen(InteropMesh* a, TetgenBehaviourUnsafe* behaviour);

        /// <summary>
        /// The DLL entry definition for freeing the memory after a CSG operation.
        /// </summary>
        /// <param name="a"></param>
        [DllImport("TetgenWrapper.dll")]
        private static unsafe extern void freeMesh(InteropMesh* a);

        [DllImport("TetgenWrapper.dll")]
        private static unsafe extern int test();

        /// <summary>
        /// Performs the specified operation on the provided meshes.
        /// </summary>
        /// <param name="vertList">The first mesh vertices as double list.</param>
        /// <param name="secondVerts">The second mesh vertices as double list.</param>
        /// <param name="faceIndexList">Face indices of first mesh as int list.</param>
        /// <param name="faceSizesList">Face sizes of first mesh as int list.</param>
        /// <param name="secondFaceIndices">Face indices of second mesh as int list.</param>
        /// <param name="secondFaceSizes">Face sizes of second mesh as int list.</param>
        /// <param name="operation">The mesh opration to perform on the two meshes</param>
        /// <returns>A triangular mesh resulting from performing the specified operation. If the resulting mesh is empty, will return null.</returns>
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public static TetgenMesh Tetrahedralize(double[] vertList, int[] faceIndexList, int[] faceSizesList, TetgenBehaviour beh)
        {
            TetgenMesh finalResult = null;

            unsafe
            {
                InteropMesh a = new InteropMesh();
                InteropMesh* result = null;

                fixed (double* aVerts = &vertList[0])
                {
                    fixed (int* aFaces = &faceIndexList[0])
                    {
                        fixed (int* aFSizes = &faceSizesList[0])
                        {
                            a.numVertices = vertList.Length / 3;
                            a.numFaceIndices = faceIndexList.Length;
                            a.vertices = aVerts;
                            a.faceIndices = aFaces;
                            a.faceSizes = aFSizes;
                            a.numFaces = faceSizesList.Length;

                            System.Console.WriteLine(string.Format("Num verts: {0}", a.numVertices));
                            System.Console.WriteLine(string.Format("Num face indices: {0}", a.numFaceIndices));
                            System.Console.WriteLine(string.Format("Num faces: {0}", a.numFaces));

                            TetgenBehaviourUnsafe bu = new TetgenBehaviourUnsafe();
                            bu.plc = beh.plc;
                            bu.quality = beh.quality;
                            bu.optlevel = beh.optlevel;
                            bu.optmaxdihedral = beh.optmaxdihedral;
                            bu.optminslidihed = beh.optminslidihed;
                            bu.optminsmtdihed = beh.optminsmtdihed;
                            bu.coarsen = beh.coarsen;
                            bu.maxvolume = beh.maxvolume;
                            bu.minratio = beh.minratio;
                            bu.mindihedral = beh.mindihedral;
                            bu.insertaddpoints = beh.insertaddpoints;
                            bu.refine = beh.refine;
                            bu.supsteiner_level = beh.supsteiner_level;


                            //try
                            //{
                                result = performTetgen(&a, &bu);
                            //}

                            //catch (SEHException ex)
                            //{
                            //    System.Console.WriteLine(ex.Message);
                            //    System.Console.WriteLine(ex.StackTrace);
                            //    System.Console.ReadLine();

                                //ArgumentException e = new ArgumentException("Tetgen has thrown an error. Possible reason is corrupt or self-intersecting meshes", ex);
                                //throw e;
                            //}
                        }
                    }
                }

                if (result == null)
                {
                    System.Console.WriteLine("Result is null.");
                    throw new Exception("TetgenSharp::Tetrahedralize => Result is null.");
                    return null;
                }

                if (result->numVertices == 0)
                {
                    freeMesh(result);
                    return null;
                }
                finalResult = new TetgenMesh();

                finalResult.Vertices = new double[result->numVertices * 3];
                finalResult.FaceIndices = new int[result->numFaceIndices];
                finalResult.FaceSizes = new int[result->numFaces];
                finalResult.TetraIndices = new int[result->numTetra * 4];
                finalResult.EdgeIndices = new int[result->numEdges * 2];

                if (result->numVertices > 0)
                    Parallel.For(0, finalResult.Vertices.Length, i =>
                    {
                        finalResult.Vertices[i] = result->vertices[i];
                    });

                if (result->numFaceIndices > 0)
                    Parallel.For(0, finalResult.FaceIndices.Length, i =>
                    {
                        finalResult.FaceIndices[i] = result->faceIndices[i];
                    });

                if (result->numFaces > 0)
                    Parallel.For(0, finalResult.FaceSizes.Length, i =>
                    {
                        finalResult.FaceSizes[i] = result->faceSizes[i];
                    });

                if (result->numTetra > 0)
                    Parallel.For(0, finalResult.TetraIndices.Length, i =>
                    {
                        finalResult.TetraIndices[i] = result->tetra[i];
                    });

                if (result->numEdges > 0)
                    Parallel.For(0, finalResult.EdgeIndices.Length, i =>
                    {
                        finalResult.EdgeIndices[i] = result->edges[i];
                    });

                //freeMesh(result);

            }   // end-unsafe

            return finalResult;
        }

        /// <summary>
        /// Performs the specified operation on the provided meshes.
        /// </summary>
        /// <param name="MeshA">The first mesh.</param>
        /// <returns>A triangular mesh resulting from performing the specified operation. If the resulting mesh is empty, will return null.</returns>
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public static TetgenMesh Tetrahedralize(TetgenMesh mesh, TetgenBehaviour Behaviour)
        {
            return Tetrahedralize(mesh.Vertices, mesh.FaceIndices, mesh.FaceSizes, Behaviour);
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public static int Test()
        {
            return test();
        }

    }
}
