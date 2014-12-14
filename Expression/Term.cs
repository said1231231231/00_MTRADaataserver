using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonClassesLib.CommonClasses;

namespace Expression
{
	/// <summary>
	/// член формулы типа Single
	/// </summary>
	public class SingleTerm : ITerm
	{
		public object TermValue 
		{
			get 
			{
				return termValue;
			}
			set 
			{
				termValue = (Single)value;
			}
		}
		Single termValue = Single.NaN;
        /// <summary>
        /// качество расчетнго тега - складывается из качеств 
        /// исходных тегов - если хотя бы одно из них плохое
        /// то и результирующее тоже плохое
        /// </summary>
        public ProjectCommonData.VarQuality VARQuality { get; set; }

        public SingleTerm()
        {
            VARQuality = ProjectCommonData.VarQuality.vqUndefined;
        }
	}
    /// <summary>
    /// член формулы типа Enum
    /// </summary>
    public class EnumTerm : ITerm
    {
        public object TermValue
        {
            get
            {
                return termValue;
            }
            set
            {
                termValue = (Single)value;
            }
        }
        Single termValue = Single.NaN;
        /// <summary>
        /// качество расчетнго тега - складывается из качеств 
        /// исходных тегов - если хотя бы одно из них плохое
        /// то и результирующее тоже плохое
        /// </summary>
        public ProjectCommonData.VarQuality VARQuality { get; set; }

        public EnumTerm()
        {
            VARQuality = ProjectCommonData.VarQuality.vqUndefined;
        }
    }

	/// <summary>
	/// член формулы типа Boolean
	/// </summary>
	public class BooleanTerm : ITerm
	{
		public object TermValue
		{
			get
			{
				return termValue;
			}
			set
			{
				termValue = (Boolean)value;
			}
		}
		Boolean termValue = Boolean.Parse(Boolean.FalseString);
        /// <summary>
        /// качество расчетнго тега - складывается из качеств 
        /// исходных тегов - если хотя бы одно из них плохое
        /// то и результирующее тоже плохое
        /// </summary>
        public ProjectCommonData.VarQuality VARQuality { get; set; }

        public BooleanTerm()
        {
            VARQuality = ProjectCommonData.VarQuality.vqUndefined;
        }
	}

    /// <summary>
    /// член-константа  типа Single формулы
    /// </summary>
    public class SingleTermConst : ITerm
    {
        public object TermValue
        {
            get
            {
                return termValue;
            }
            set
            {
                termValue = (Single)value;
            }
        }
        Single termValue = Single.NaN;
        /// <summary>
        /// качество расчетнго тега - складывается из качеств 
        /// исходных тегов - если хотя бы одно из них плохое
        /// то и результирующее тоже плохое
        /// </summary>
        public ProjectCommonData.VarQuality VARQuality { get; set; }

        public SingleTermConst()
        {
            VARQuality = ProjectCommonData.VarQuality.vqGood;//.vqUndefined;
        }
    }

    /// <summary>
    /// член-константа типа Boolean формулы 
    /// </summary>
    public class BooleanTermConst : ITerm
    {
        public object TermValue
        {
            get
            {
                return termValue;
            }
            set
            {
                termValue = (Boolean)value;
            }
        }
        Boolean termValue = Boolean.Parse(Boolean.FalseString);
        /// <summary>
        /// качество расчетнго тега - складывается из качеств 
        /// исходных тегов - если хотя бы одно из них плохое
        /// то и результирующее тоже плохое
        /// </summary>
        public ProjectCommonData.VarQuality VARQuality { get; set; }

        public BooleanTermConst()
        {
            VARQuality = ProjectCommonData.VarQuality.vqGood;//.vqUndefined;
        }
    }
}