# Tetrino

"TetGen is a program to generate tetrahedral meshes of any 3D polyhedral domains. TetGen generates exact constrained Delaunay tetrahedralizations, boundary conforming Delaunay meshes, and Voronoi partitions." (http://wias-berlin.de/software/index.jsp?id=TetGen&lang=1)

TetRhino (or Tetrino) is a .NET wrapper for the well-known and pretty amazing TetGen mesh tetrahedralization program. It provides one new GH component for discretizing or remeshing objects using TetGen. Basic tetrahedralization functionality is exposed with a few different output types that can be controlled. At the moment, the only control for tetrahedra sizes is the minimum ratio, which is controlled by a slider. This is hardcoded to always be above 1.0-1.1, as it is very easy to generate a LOT of data (and crash)...

The libs are divided again into different modules to allow flexibility and fun with or without Rhino and GH, so have fun. All 4 libs should be placed in a folder (maybe called 'tetgen') in your GH libraries folder. Remember to unblock.

Once again, the libs are provided as-is, with no guarantee of support for now, as I use them internally and do not intend to develop this into a shiny, polished plug-in. If there is enough interest, I can tidy up the code-base and upload it somewhere if someone more savvy than me wants to play.

**TetgenGH.gha** - Grasshopper assembly which adds the 'Tetrahedralize' component to Mesh -> Triangulation.

**TetgenRC.dll** - RhinoCommon interface to the Tetgen wrapper.

**TetgenSharp.dll** - dotNET wrapper for Tetgen.

**TetgenWrapper.dll** - Actual wrapper for Tetgen.

Obviously, credit where credit is due for this excellent and tiny piece of software: 

"The development of TetGen is executed at the Weierstrass Institute for Applied Analysis and Stochastics in the research group of Numerical Mathematics and Scientific Computing." See http://wias-berlin.de/software/index.jsp?id=TetGen&lang=1 for more details about TetGen.

To wrap up, some notes about the inputs:

These are the possible integer Flags (F) values and resultant outputs for the GH component:

**0** - Output M yields a closed boundary mesh. Useful for simply remeshing your input mesh.

**1** - Output M yields a list of tetra meshes.

**2** - Output I yields a DataTree of tetra indices, grouped in lists of 4. Output P yields a list of points to which the tetra indices correspond.

**3** - Output I yields a DataTree of edge indices, grouped in lists of 2. Output P yields a list of points to which the edge indices correspond. Useful for lots of things, very easy to create lines from this to plug into K2 or something for some ropey FEA (or not so ropey!) ;)

As this component can potentially create a LOT of data, especially with dense meshes, care should be taken with the MinRatio (R) input. This will try to constrain the tetra to be more or less elongated, which also means that the lower this value gets, the more tetra need to be added to satisfy this constraint. Start with very high values and lower them until satisfactory.

Happy tetrahedralizing...

# To do
- Revise GH component output to better handle multiple inputs (output nicely organized data trees).
- Look at exposing more control options for Tetgen.

# Contact

tsvi@kadk.dk

http://tomsvilans.com
