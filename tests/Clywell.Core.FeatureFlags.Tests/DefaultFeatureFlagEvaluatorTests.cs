namespace Clywell.Core.FeatureFlags.Tests;

public sealed class DefaultFeatureFlagEvaluatorTests
{
    [Fact]
    public void Evaluate_NoRulesDefaultTrue_ReturnsDefaultValue()
    {
        var flag = new FeatureFlag { Key = "flag", DefaultValue = true };
        var sut = new DefaultFeatureFlagEvaluator();

        var result = sut.Evaluate(flag, EvaluationContext.Empty);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_NoRulesDefaultFalse_ReturnsDefaultValue()
    {
        var flag = new FeatureFlag { Key = "flag", DefaultValue = false };
        var sut = new DefaultFeatureFlagEvaluator();

        var result = sut.Evaluate(flag, EvaluationContext.Empty);

        Assert.False(result);
    }

    [Fact]
    public void Evaluate_SingleMatchingRule_ReturnsRuleValue()
    {
        var flag = new FeatureFlag
        {
            Key = "flag",
            DefaultValue = false,
            Rules =
            [
                new EvaluationRule
                {
                    Priority = 1,
                    Condition = AlwaysCondition.Instance,
                    Value = true
                }
            ]
        };

        var sut = new DefaultFeatureFlagEvaluator();

        var result = sut.Evaluate(flag, EvaluationContext.Empty);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_SingleNonMatchingRule_ReturnsDefaultValue()
    {
        var condition = new Mock<IEvaluationCondition>();
        condition
            .Setup(c => c.Matches(It.IsAny<EvaluationContext>()))
            .Returns(false);

        var flag = new FeatureFlag
        {
            Key = "flag",
            DefaultValue = true,
            Rules =
            [
                new EvaluationRule
                {
                    Priority = 1,
                    Condition = condition.Object,
                    Value = false
                }
            ]
        };

        var sut = new DefaultFeatureFlagEvaluator();

        var result = sut.Evaluate(flag, EvaluationContext.Empty);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_TwoMatchingRules_HigherPriorityWins()
    {
        var lowCondition = new Mock<IEvaluationCondition>();
        lowCondition
            .Setup(c => c.Matches(It.IsAny<EvaluationContext>()))
            .Returns(true);

        var highCondition = new Mock<IEvaluationCondition>();
        highCondition
            .Setup(c => c.Matches(It.IsAny<EvaluationContext>()))
            .Returns(true);

        var flag = new FeatureFlag
        {
            Key = "flag",
            DefaultValue = false,
            Rules =
            [
                new EvaluationRule { Priority = 1, Condition = lowCondition.Object, Value = false },
                new EvaluationRule { Priority = 10, Condition = highCondition.Object, Value = true }
            ]
        };

        var sut = new DefaultFeatureFlagEvaluator();

        var result = sut.Evaluate(flag, EvaluationContext.Empty);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_HighestPriorityDoesNotMatchSecondMatches_ReturnsSecondValue()
    {
        var highCondition = new Mock<IEvaluationCondition>();
        highCondition
            .Setup(c => c.Matches(It.IsAny<EvaluationContext>()))
            .Returns(false);

        var secondCondition = new Mock<IEvaluationCondition>();
        secondCondition
            .Setup(c => c.Matches(It.IsAny<EvaluationContext>()))
            .Returns(true);

        var flag = new FeatureFlag
        {
            Key = "flag",
            DefaultValue = false,
            Rules =
            [
                new EvaluationRule { Priority = 20, Condition = highCondition.Object, Value = false },
                new EvaluationRule { Priority = 10, Condition = secondCondition.Object, Value = true }
            ]
        };

        var sut = new DefaultFeatureFlagEvaluator();

        var result = sut.Evaluate(flag, EvaluationContext.Empty);

        Assert.True(result);
    }

    [Fact]
    public void Evaluate_RulesOutOfPriorityOrder_EvaluatesInDescendingPriorityOrder()
    {
        var highPriorityCondition = new Mock<IEvaluationCondition>(MockBehavior.Strict);
        var lowPriorityCondition = new Mock<IEvaluationCondition>(MockBehavior.Strict);
        var sequence = new MockSequence();

        highPriorityCondition
            .InSequence(sequence)
            .Setup(c => c.Matches(It.IsAny<EvaluationContext>()))
            .Returns(false);

        lowPriorityCondition
            .InSequence(sequence)
            .Setup(c => c.Matches(It.IsAny<EvaluationContext>()))
            .Returns(false);

        var flag = new FeatureFlag
        {
            Key = "flag",
            DefaultValue = true,
            Rules =
            [
                new EvaluationRule { Priority = 1, Condition = lowPriorityCondition.Object, Value = false },
                new EvaluationRule { Priority = 100, Condition = highPriorityCondition.Object, Value = false }
            ]
        };

        var sut = new DefaultFeatureFlagEvaluator();

        var result = sut.Evaluate(flag, EvaluationContext.Empty);

        Assert.True(result);
        highPriorityCondition.Verify(c => c.Matches(It.IsAny<EvaluationContext>()), Times.Once);
        lowPriorityCondition.Verify(c => c.Matches(It.IsAny<EvaluationContext>()), Times.Once);
    }

    [Fact]
    public void Evaluate_NullFlag_ThrowsArgumentNullException()
    {
        var sut = new DefaultFeatureFlagEvaluator();

        Assert.Throws<ArgumentNullException>(() => sut.Evaluate(null!, EvaluationContext.Empty));
    }

    [Fact]
    public void Evaluate_NullContext_ThrowsArgumentNullException()
    {
        var sut = new DefaultFeatureFlagEvaluator();
        var flag = new FeatureFlag { Key = "flag", DefaultValue = false };

        Assert.Throws<ArgumentNullException>(() => sut.Evaluate(flag, null!));
    }
}