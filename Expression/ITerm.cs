using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonClassesLib.CommonClasses;

namespace Expression
{
	public interface ITerm
	{
		object TermValue { get; set; }
        /// <summary>
        /// качество расчетнго тега - складывается из качеств 
        /// исходных тегов - если хотя бы одно из них плохое
        /// то и результирующее тоже плохое
        /// </summary>
        ProjectCommonData.VarQuality VARQuality { get; set; }
	}
}
