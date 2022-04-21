#ifdef NEW_WRAPPER
#include "Wrapper.h"

namespace TetgenWrapper
{
	tetgenbehavior* tetgenbehavior_create()
	{
		return new tetgenbehavior();
	}

	void tetgenbehavior_delete(tetgenbehavior* ptr)
	{

		delete ptr;
	}

	void tetgenbehavior_set_plc(tetgenbehavior* ptr, int plc) { ptr->plc = plc; }
	void tetgenbehavior_set_refine(tetgenbehavior* ptr, int refine) { ptr->refine = refine; }
	void tetgenbehavior_set_quality(tetgenbehavior* ptr, int quality) { ptr->quality = quality; }
	void tetgenbehavior_set_coarsen(tetgenbehavior* ptr, int coarsen) { ptr->coarsen = coarsen; }
	void tetgenbehavior_set_insertaddpoints(tetgenbehavior* ptr, int insertaddpoints) { ptr->insertaddpoints = insertaddpoints; }

	void tetgenbehavior_set_maxvolume(tetgenbehavior* ptr, double maxvolume) { ptr->maxvolume = maxvolume; }
	void tetgenbehavior_set_minratio(tetgenbehavior* ptr, double minratio) { ptr->minratio = minratio; }
	void tetgenbehavior_set_mindihedral(tetgenbehavior* ptr, double mindihedral) { ptr->mindihedral = mindihedral; }
	void tetgenbehavior_set_optmaxdihedral(tetgenbehavior* ptr, double optmaxdihedral) { ptr->optmaxdihedral = optmaxdihedral; }
}
#endif
