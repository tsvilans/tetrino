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
using System.Drawing;
using Grasshopper.Kernel;

namespace TetgenGH
{
    public class TetgenGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "TetgenGH";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.TetgenGH_icon;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "This provides an extra GH component which uses Tetgen to tetrahedralize a mesh.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("e04f470e-7580-49b8-bdcf-8e83c5f85c99");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Tom Svilans";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "tsvi@kadk.dk";
            }
        }
    }
}
