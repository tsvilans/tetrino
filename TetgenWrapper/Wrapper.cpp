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

#include "Wrapper.h"

InteropMesh* performTetgen(InteropMesh* mesh, TetgenBehaviour* tb)
{
	//return mesh;
	//tetrahedralize(be)
	
	tetgenbehavior beh;
	beh.quality				= tb->quality;
	beh.plc					= tb->plc;
	beh.refine				= tb->refine;
	beh.maxvolume			= tb->maxvolume;
	beh.insertaddpoints		= tb->insertaddpoints;
	beh.optlevel			= tb->optlevel;
	beh.optminslidihed		= tb->optminslidihed;
	beh.optminsmtdihed		= tb->optminsmtdihed;
	beh.quiet				= 0;
	beh.verbose				= 1;
	beh.supsteiner_level	= tb->supsteiner_level;
	beh.minratio			= tb->minratio;
	beh.coarsen				= tb->coarsen;
	beh.mindihedral			= tb->mindihedral;
	beh.edgesout			= 1;
	if (tb->maxvolume > 0.0)
	{
		beh.varvolume = 1;
		beh.fixedvolume = 1;
	}
	beh.regionattrib		= 1;


	tetgenio* in = io_to_tetgenio(mesh);
	std::cout << "Tetgen :: Num verts in: " << in->numberofpoints << std::endl;
	tetgenio* out = new tetgenio();

	// XXXXXXX
	// Detect improper combinations of switches.

	if (beh.nobisect && (!beh.plc && !beh.refine)) {
		beh.plc = 1;
	}
	if (beh.quality && (!beh.plc && !beh.refine)) {
		beh.plc = 1;
	}
	if (beh.diagnose && !beh.plc) {
		beh.plc = 1;
	}
	if (beh.refine && !beh.quality) {
		beh.optlevel = 0;
	}
	if (beh.insertaddpoints && (beh.optlevel == 0)) {
		beh.optlevel = 2;
	}
	if (beh.coarsen && (beh.optlevel == 0)) {
		beh.optlevel = 2;
	}

	if ((beh.refine || beh.plc) && beh.weighted) {
		printf("Error:  Switches -w cannot use together with -p or -r.\n");
		return false;
	}

	if (beh.convex) { // -c
		if (beh.plc && !beh.regionattrib) {
			beh.regionattrib = 1;
		}
	}

	if (beh.refine || !beh.plc) {
		beh.regionattrib = 0;
	}

	if (!beh.refine && !beh.plc) {
		beh.varvolume = 0;
	}
	if (beh.fixedvolume || beh.varvolume) {
		if (beh.quality == 0) {
			beh.quality = 1;
			if (!beh.plc && !beh.refine) {
				beh.plc = 1;
			}
		}
	}

	if (!beh.quality) {
		if (beh.optmaxdihedral < 179.0) {
			if (beh.nobisect) {  // with -Y option
				beh.optmaxdihedral = 179.0;
			}
			else {
				beh.optmaxdihedral = 179.999;
			}
		}
		if (beh.optminsmtdihed < 179.999) {
			beh.optminsmtdihed = 179.999;
		}
		if (beh.optminslidihed < 179.999) {
			beh.optminslidihed = 179.999;
		}
	}
	// XXXXXXX


	tetrahedralize(&beh, in, out);


	std::cout << "Tetgen :: Num verts out: " << out->numberofpoints << std::endl;
	//return mesh;

//	delete mesh;
	mesh = tetgenio_to_io(out);

	return mesh;
	
}

int test()
{
	return 99;
}

tetgenio* io_to_tetgenio(InteropMesh* mesh)
{
	tetgenio* io = new tetgenio();
	tetgenmesh::arraypool *plist;
	tetgenio::facet *f;
	tetgenio::polygon *p;

	//double *coord;
	int solid = 0;
	int nverts = mesh->numVertices, iverts = 0;
	int nfaces = mesh->numFaces;
	io->numberofpoints = nverts;

	std::cout << "Tetgen :: io_to_tetgenio :: nverts: " << nverts << std::endl;
	std::cout << "Tetgen :: io_to_tetgenio :: nfaces: " << nfaces << std::endl;

	int nverts3 = nverts * 3;
	io->pointlist = new REAL[nverts3];


	for (int i = 0; i < nverts3; i+= 3) {
		//coord = (double *)&mesh->vertices[i];
		//iverts = i * 3;
		/*
		io->pointlist[iverts] = (REAL)coord[0];
		io->pointlist[iverts + 1] = (REAL)coord[1];
		io->pointlist[iverts + 2] = (REAL)coord[2];
		*/
		io->pointlist[i] = mesh->vertices[i];
		io->pointlist[i + 1] = mesh->vertices[i + 1];
		io->pointlist[i + 2] = mesh->vertices[i + 2];
	}

	//nfaces = (int)(nverts / 3);
	io->numberoffacets = nfaces;
	io->facetlist = new tetgenio::facet[nfaces];

	// Default use '1' as the array starting index.
	io->firstnumber = 0;
	iverts = io->firstnumber;
	int fi = 0;
	for (int i = 0; i < nfaces; i++) {
		f = &io->facetlist[i];
		io->init(f);
		// In .stl format, each facet has one polygon, no hole.
		f->numberofpolygons = 1;
		f->polygonlist = new tetgenio::polygon[1];
		p = &f->polygonlist[0];
		io->init(p);
		// Each polygon has three vertices.
		p->numberofvertices = mesh->faceSizes[i];
		p->vertexlist = new int[p->numberofvertices];
		for (int j = 0; j < p->numberofvertices; ++j)
		{
			p->vertexlist[j] = mesh->faceIndices[fi]; fi++;
		}
		//fi += p->numberofvertices;
	}

	return io;
}


InteropMesh* tetgenio_to_io(tetgenio* io)
{
	InteropMesh* mesh = new InteropMesh();

	std::cout << "Num tetra: " << io->numberoftetrahedra << std::endl;
	std::cout << "Num vpoints: " << io->numberofvpoints << std::endl;
	std::cout << "Num vcells: " << io->numberofvcells << std::endl;

	/*
	int ii = 0;
	for (int i = 0; i < io->numberoftetrahedra; ++i)
	{
		ii = i * 4;
		std::cout << "T: " << io->tetrahedronlist[ii] << " " <<
			io->tetrahedronlist[ii + 1] << " " << 
			io->tetrahedronlist[ii + 2] << " " << 
			io->tetrahedronlist[ii + 3] << " " << 
			std::endl;

	}
	*/

	mesh->numTetra = io->numberoftetrahedra;
	mesh->tetra = new int[mesh->numTetra * 4];

	for (int i = 0; i < mesh->numTetra * 4; ++i)
	{
		mesh->tetra[i] = io->tetrahedronlist[i];
	}
	

	mesh->numFaces = io->numberoftrifaces;
	mesh->faceIndices = new int[mesh->numFaces * 3];
	mesh->faceSizes = new int[mesh->numFaces];
	mesh->numFaceIndices = mesh->numFaces * 3;

	mesh->numVertices = io->numberofpoints;
	mesh->vertices = new double[mesh->numVertices * 3];

	mesh->numEdges = io->numberofedges;
	mesh->edges = new int[mesh->numEdges * 2];

	int ivert = 0;
	for (int i = 0; i < io->numberofpoints; ++i)
	{
		ivert = i * 3;
		mesh->vertices[ivert] = io->pointlist[ivert];
		mesh->vertices[ivert + 1] = io->pointlist[ivert + 1];
		mesh->vertices[ivert + 2] = io->pointlist[ivert + 2];
	}

	for (int i = 0; i < io->numberoftrifaces; ++i)
	{
		ivert = i * 3;
		mesh->faceIndices[ivert] = io->trifacelist[ivert];
		mesh->faceIndices[ivert + 1] = io->trifacelist[ivert + 1];
		mesh->faceIndices[ivert + 2] = io->trifacelist[ivert + 2];
	}

	for (int i = 0; i < io->numberoftrifaces; ++i)
	{
		mesh->faceSizes[i] = 3;
	}

	for (int i = 0; i < io->numberofedges * 2; ++i)
	{
		mesh->edges[i] = io->edgelist[i];
	}

	return mesh;
}

/*
bool tetgenio::load_stl(char* filebasename)
{
	
	FILE *fp;
	tetgenmesh::arraypool *plist;
	tetgenio::facet *f;
	tetgenio::polygon *p;
	char infilename[FILENAMESIZE];
	char buffer[INPUTLINESIZE];
	char *bufferp, *str;
	double *coord;
	int solid = 0;
	int nverts = 0, iverts = 0;
	int nfaces = 0;
	int line_count = 0, i;

	strncpy(infilename, filebasename, FILENAMESIZE - 1);
	infilename[FILENAMESIZE - 1] = '\0';
	if (infilename[0] == '\0') {
		printf("Error:  No filename.\n");
		return false;
	}
	if (strcmp(&infilename[strlen(infilename) - 4], ".stl") != 0) {
		strcat(infilename, ".stl");
	}

	if (!(fp = fopen(infilename, "r"))) {
		printf("Error:  Unable to open file %s\n", infilename);
		return false;
	}
	printf("Opening %s.\n", infilename);

	// STL file has no number of points available. Use a list to read points.
	plist = new tetgenmesh::arraypool(sizeof(double) * 3, 10);

	while ((bufferp = readline(buffer, fp, &line_count)) != NULL) {
		// The ASCII .stl file must start with the lower case keyword solid and
		//   end with endsolid.
		if (solid == 0) {
			// Read header 
			bufferp = strstr(bufferp, "solid");
			if (bufferp != NULL) {
				solid = 1;
			}
		}
		else {
			// We're inside the block of the solid.
			str = bufferp;
			// Is this the end of the solid.
			bufferp = strstr(bufferp, "endsolid");
			if (bufferp != NULL) {
				solid = 0;
			}
			else {
				// Read the XYZ coordinates if it is a vertex.
				bufferp = str;
				bufferp = strstr(bufferp, "vertex");
				if (bufferp != NULL) {
					plist->newindex((void **)&coord);
					for (i = 0; i < 3; i++) {
						bufferp = findnextnumber(bufferp);
						if (*bufferp == '\0') {
							printf("Syntax error reading vertex coords on line %d\n",
								line_count);
							delete plist;
							fclose(fp);
							return false;
						}
						coord[i] = (REAL)strtod(bufferp, &bufferp);
					}
				}
			}
		}
	}
	fclose(fp);

	nverts = (int)plist->objects;
	// nverts should be an integer times 3 (every 3 vertices denote a face).
	if (nverts == 0 || (nverts % 3 != 0)) {
		printf("Error:  Wrong number of vertices in file %s.\n", infilename);
		delete plist;
		return false;
	}
	numberofpoints = nverts;
	pointlist = new REAL[nverts * 3];
	for (i = 0; i < nverts; i++) {
		coord = (double *)fastlookup(plist, i);
		iverts = i * 3;
		pointlist[iverts] = (REAL)coord[0];
		pointlist[iverts + 1] = (REAL)coord[1];
		pointlist[iverts + 2] = (REAL)coord[2];
	}

	nfaces = (int)(nverts / 3);
	numberoffacets = nfaces;
	facetlist = new tetgenio::facet[nfaces];

	// Default use '1' as the array starting index.
	firstnumber = 1;
	iverts = firstnumber;
	for (i = 0; i < nfaces; i++) {
		f = &facetlist[i];
		init(f);
		// In .stl format, each facet has one polygon, no hole.
		f->numberofpolygons = 1;
		f->polygonlist = new tetgenio::polygon[1];
		p = &f->polygonlist[0];
		init(p);
		// Each polygon has three vertices.
		p->numberofvertices = 3;
		p->vertexlist = new int[p->numberofvertices];
		p->vertexlist[0] = iverts;
		p->vertexlist[1] = iverts + 1;
		p->vertexlist[2] = iverts + 2;
		iverts += 3;
	}

	delete plist;
	return true;
}
*/

void freeMesh(InteropMesh* mesh)
{
	mesh->free();
}
