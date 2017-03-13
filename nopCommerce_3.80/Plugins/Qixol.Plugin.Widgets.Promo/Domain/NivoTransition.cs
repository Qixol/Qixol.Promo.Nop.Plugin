using System;

namespace Qixol.Plugin.Widgets.Promo.Domain
{
    public class NivoTransition
    {
        #region "constants"

        public static Type STACKHORIZONTAL
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.stackHorizontal", TransitionType = "stackHorizontal" }; }
        }

        public static Type STACKVERTICAL
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.stackVertical", TransitionType = "stackVertical" }; }
        }

        public static Type SLICEDOWN
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.sliceDown", TransitionType = "sliceDown" }; }
        }

        public static Type SLICEDOWNLEFT
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.sliceDownLeft", TransitionType = "sliceDownLeft" }; }
        }

        public static Type SLICEUP
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.sliceUp", TransitionType = "sliceUp" }; }
        }

        public static Type SLICEUPLEFT
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.sliceUpLeft", TransitionType = "sliceUpLeft" }; }
        }

        public static Type SLICEUPDOWN
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.sliceUpDown", TransitionType = "sliceUpDown" }; }
        }

        public static Type SLICEUPDOWNLEFT
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.sliceUpDownLeft", TransitionType = "sliceUpDownLeft" }; }
        }

        public static Type FOLD
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.fold", TransitionType = "fold" }; }
        }

        public static Type FADE
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.fade", TransitionType = "fade" }; }
        }

        public static Type RANDOM
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.random", TransitionType = "random" }; }
        }

        public static Type SLIDEINRIGHT
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.slideInRight", TransitionType = "slideInRight" }; }
        }

        public static Type SLIDEINLEFT
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.slideInLeft", TransitionType = "slideInLeft" }; }
        }

        public static Type BOXRANDOM
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.boxRandom", TransitionType = "boxRandom" }; }
        }

        public static Type BOXRAIN
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.boxRain", TransitionType = "boxRain" }; }
        }

        public static Type BOXRAINREVERSE
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.boxRainReverse", TransitionType = "boxRainReverse" }; }
        }

        public static Type BOXRAINGROW
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.boxRainGrow", TransitionType = "boxRainGrow" }; }
        }

        public static Type BOXRAINGROWREVERSE
        {
            get { return new Type() { ResourceName = "Plugins.Widgets.QixolPromo.TransitionType.boxRainGrowReverse", TransitionType = "boxRainGrowReverse" }; }
        }

        #endregion

        #region methods
        
        public static Type FindByTransitionType(string transitionType)
        {
            if (string.Compare(transitionType, STACKHORIZONTAL.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return STACKHORIZONTAL;

            if (string.Compare(transitionType, STACKVERTICAL.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return STACKVERTICAL;

            if (string.Compare(transitionType, SLICEDOWN.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return SLICEDOWN;

            if (string.Compare(transitionType, SLICEDOWNLEFT.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return SLICEDOWNLEFT;

            if (string.Compare(transitionType, SLICEUP.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return SLICEUP;

            if (string.Compare(transitionType, SLICEUPLEFT.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return SLICEUPLEFT;

            if (string.Compare(transitionType, SLICEUPDOWN.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return SLICEUPDOWN;

            if (string.Compare(transitionType, SLICEUPDOWNLEFT.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return SLICEUPDOWNLEFT;

            if (string.Compare(transitionType, FOLD.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return FOLD;

            if (string.Compare(transitionType, FADE.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return FADE;

            if (string.Compare(transitionType, RANDOM.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return RANDOM;

            if (string.Compare(transitionType, SLIDEINRIGHT.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return SLIDEINRIGHT;

            if (string.Compare(transitionType, SLIDEINLEFT.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return SLIDEINLEFT;

            if (string.Compare(transitionType, BOXRANDOM.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return BOXRANDOM;

            if (string.Compare(transitionType, BOXRAIN.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return BOXRAIN;

            if (string.Compare(transitionType, BOXRAINREVERSE.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return BOXRAINREVERSE;

            if (string.Compare(transitionType, BOXRAINGROW.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return BOXRAINGROW;

            if (string.Compare(transitionType, BOXRAINGROWREVERSE.TransitionType, StringComparison.InvariantCultureIgnoreCase) == 0)
                return BOXRAINGROWREVERSE;

            return null;
        }

        #endregion

        #region nested classes

        public class Type
        {
            public string ResourceName { get; set; }
            public string TransitionType { get; set; }
        }

        #endregion
    }
}
