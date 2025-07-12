using System;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace apiTest.Fixtures;

public class TestFixture : Fixture
{
    private static readonly Random _random = new();

    public TestFixture()
    {
        Customize(new AutoMoqCustomization());
        Behaviors.Remove(new ThrowingRecursionBehavior());
        Behaviors.Add(new OmitOnRecursionBehavior());
        LimitDecimalMaxRange();
    }

    /// <summary>
    ///   Represents a custom AutoFixture fixture for test setup.
    ///   This class includes common customizations, such as limiting the range
    ///   of generated decimal values to prevent `OverflowException`s
    ///   when dealing with properties constrained by large `double.MaxValue` ranges.
    /// </summary>
    private void LimitDecimalMaxRange()
    {
        this.Customize<int>(composer => composer.FromFactory(() => _random.Next(1, 101)));
    }
}
