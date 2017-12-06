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

#ifndef WRAPPER_H
#define WRAPPER_H

#include <cstdlib>
#include "tetgen.h"

#include <iostream>

struct InteropMesh
{
	double *vertices; // numVertices * 3
	int *faceIndices; 
	int *faceSizes;
	int *tetra; // numTetra * 4
	int *edges; // numEdges * 2
	int numVertices;
	int numFaces;
	int numFaceIndices;
	int numTetra;
	int numEdges;

	void free()
	{
		delete[] vertices;
		delete[] faceIndices;
		delete[] faceSizes;
		delete[] tetra;
		delete[] edges;
	}
};

struct TetgenBehaviour
{
	int plc = 0;
	int refine = 0;
	int quality = 0;
	int coarsen = 0;
	int insertaddpoints = 0;

	int supsteiner_level = 2;
	int optlevel = 2;

	double maxvolume = -1.0;
	double minratio = 2.0;
	double mindihedral = 0.0;

	double optmaxdihedral = 165.00;
	double optminslidihed = 179.0;
	double optminsmtdihed = 179.0;
};

extern "C" __declspec(dllexport) InteropMesh* performTetgen(InteropMesh* mesh, TetgenBehaviour* behaviour);

extern "C" __declspec(dllexport) void freeMesh(InteropMesh* mesh);

extern "C" __declspec(dllexport) int test();

tetgenio* io_to_tetgenio(InteropMesh* mesh);

InteropMesh* tetgenio_to_io(tetgenio* io);

#endif