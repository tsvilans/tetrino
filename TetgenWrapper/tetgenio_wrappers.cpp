#ifdef NEW_WRAPPER
#include "Wrapper.h"
namespace TetgenWrapper
{
	tetgenio* tetgenio_create()
	{
		return new tetgenio();
	}

	void tetgenio_delete(tetgenio* ptr)
	{
		delete ptr;
	}
}
#endif