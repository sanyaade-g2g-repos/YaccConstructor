﻿using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;

namespace ApproximatorTester.Tests
{
    [TestFixture]
    public class ExecuteTests : CSharpContextActionExecuteTestBase<RunApproximatorAction>
    {
        protected override string ExtraPath
        {
            get { return "AllTests"; }
        }

        protected override string RelativeTestDataPath
        {
            get { return "AllTests"; }
        }

        [Test]
        public void TestSimpleQuery()
        {
            DoTestFiles("SimpleQuery.cs");
        }

        [Test]
        public void TestFourVars()
        {
            DoTestFiles("FourVars.cs");
        }
    }
}