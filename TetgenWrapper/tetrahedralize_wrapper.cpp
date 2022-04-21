#ifdef NEW_WRAPPER
#include "Wrapper.h"

namespace TetgenWrapper
{
	tetgenio* tetrahedralize(tetgenbehavior* behaviour, tetgenio* in)
	{
		tetgenio* out = new tetgenio();
		tetrahedralize(behaviour, in, out);

		return out;
	}
}
#endif