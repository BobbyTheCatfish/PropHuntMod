using PropHuntMod.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PropHuntMod.Tests
{
    internal class TestNextProp : BaseTest
    {
        public override KeyCode KeyCode => KeyCode.O;
        public override string Name => "Next Prop";
        public override bool Enabled => true;

        public override bool Execute()
        {
            PropTesting.PropNext();
            return true;
        }
    }

    internal class TestPrevProp : BaseTest
    {
        public override KeyCode KeyCode => KeyCode.U;
        public override string Name => "Previous Prop";
        public override bool Enabled => true;

        public override bool Execute()
        {
            PropTesting.PropPrevious();
            return true;
        }
    }
}
