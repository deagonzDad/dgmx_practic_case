using System;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace apiTest.Fixtures;

public class TestFixture : Fixture
{
    public TestFixture()
    {
        Customize(new AutoMoqCustomization());
        Behaviors.Remove(new ThrowingRecursionBehavior());
        Behaviors.Add(new OmitOnRecursionBehavior());
    }
}
