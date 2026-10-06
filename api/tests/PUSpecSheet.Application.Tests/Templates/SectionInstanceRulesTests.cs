using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Tests.Templates;

/// <summary>How many copies of a section (or column block) a table may have, must have and starts with.</summary>
public sealed class SectionInstanceRulesTests
{
    [Theory]
    [InlineData(0, null, 0)]
    [InlineData(0, null, 5)]
    [InlineData(1, 3, 1)]
    [InlineData(1, 3, 3)]
    [InlineData(2, 2, 2)]
    public void StartingBetweenTheFewestAndTheMost_IsValid(int min, int? max, int initial)
    {
        SectionInstanceRules.EnsureValid(min, max, initial);
    }

    [Theory]
    [InlineData(2, null, 1)]
    [InlineData(0, 3, 4)]
    public void StartingOutsideTheLimits_IsRefused(int min, int? max, int initial)
    {
        Assert.Throws<InvalidRequestException>(() => SectionInstanceRules.EnsureValid(min, max, initial));
    }

    [Fact]
    public void TheHeader_IsAlwaysExactlyOne()
    {
        var section = new TemplateSection { MinInstances = 0, MaxInstances = 9, InitialInstances = 4 };

        SectionInstanceRules.ApplyHeader(section);

        Assert.Equal(SectionRole.Header, section.Role);
        Assert.Equal((1, 1, 1), (section.MinInstances, section.MaxInstances, section.InitialInstances));
    }

    [Fact]
    public void ANewAddableSection_IsOptionalAndUnlimited()
    {
        var section = new TemplateSection();

        SectionInstanceRules.ApplyNewAddable(section);

        Assert.Equal(SectionRole.Addable, section.Role);
        Assert.Equal((0, null, 0), (section.MinInstances, section.MaxInstances, section.InitialInstances));
    }

    [Fact]
    public void AnAddableSection_TakesTheRequestedLimits_OnceTheyAreChecked()
    {
        var section = new TemplateSection();

        SectionInstanceRules.ApplyAddable(new UpdateTemplateSectionRequest("Body", 1, 4, 2), section);
        Assert.Equal((1, 4, 2), (section.MinInstances, section.MaxInstances, section.InitialInstances));

        Assert.Throws<InvalidRequestException>(
            () => SectionInstanceRules.ApplyAddable(new UpdateTemplateSectionRequest("Body", 3, 4, 2), section));
    }
}
