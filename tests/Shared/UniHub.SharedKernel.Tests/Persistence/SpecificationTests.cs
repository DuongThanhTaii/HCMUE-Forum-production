using FluentAssertions;
using UniHub.SharedKernel.Persistence;

namespace UniHub.SharedKernel.Tests.Persistence;

public class SpecificationTests
{
    [Fact]
    public void ToExpression_ShouldReturnCorrectExpression()
    {
        // Arrange
        var specification = new NameEqualsSpecification("Test");

        // Act
        var expression = specification.ToExpression();
        var compiledExpression = expression.Compile();

        // Assert
        compiledExpression(new TestEntity { Name = "Test" }).Should().BeTrue();
        compiledExpression(new TestEntity { Name = "Other" }).Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_ShouldReturnTrue_WhenEntityMatchesSpecification()
    {
        // Arrange
        var specification = new NameEqualsSpecification("Test");
        var entity = new TestEntity { Name = "Test" };

        // Act
        var result = specification.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_ShouldReturnFalse_WhenEntityDoesNotMatchSpecification()
    {
        // Arrange
        var specification = new NameEqualsSpecification("Test");
        var entity = new TestEntity { Name = "Other" };

        // Act
        var result = specification.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void And_ShouldCombineSpecificationsWithAndLogic()
    {
        // Arrange
        var spec1 = new NameStartsWithSpecification("T");
        var spec2 = new AgeLessThanSpecification(30);
        var combined = spec1.And(spec2);

        // Act & Assert
        combined.IsSatisfiedBy(new TestEntity { Name = "Test", Age = 25 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestEntity { Name = "Test", Age = 35 }).Should().BeFalse();
        combined.IsSatisfiedBy(new TestEntity { Name = "Other", Age = 25 }).Should().BeFalse();
    }

    [Fact]
    public void Or_ShouldCombineSpecificationsWithOrLogic()
    {
        // Arrange
        var spec1 = new NameEqualsSpecification("Test");
        var spec2 = new NameEqualsSpecification("Other");
        var combined = spec1.Or(spec2);

        // Act & Assert
        combined.IsSatisfiedBy(new TestEntity { Name = "Test" }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestEntity { Name = "Other" }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestEntity { Name = "Different" }).Should().BeFalse();
    }

    [Fact]
    public void Not_ShouldNegateSpecification()
    {
        // Arrange
        var specification = new NameEqualsSpecification("Test");
        var negated = specification.Not();

        // Act & Assert
        negated.IsSatisfiedBy(new TestEntity { Name = "Test" }).Should().BeFalse();
        negated.IsSatisfiedBy(new TestEntity { Name = "Other" }).Should().BeTrue();
    }

    [Fact]
    public void ComplexSpecification_ShouldWorkCorrectly()
    {
        // Arrange
        // (Name starts with "T" AND Age < 30) OR Name equals "Special"
        var spec1 = new NameStartsWithSpecification("T");
        var spec2 = new AgeLessThanSpecification(30);
        var spec3 = new NameEqualsSpecification("Special");
        var combined = spec1.And(spec2).Or(spec3);

        // Act & Assert
        combined.IsSatisfiedBy(new TestEntity { Name = "Test", Age = 25 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestEntity { Name = "Test", Age = 35 }).Should().BeFalse();
        combined.IsSatisfiedBy(new TestEntity { Name = "Special", Age = 100 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestEntity { Name = "Other", Age = 25 }).Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ShouldConvertToExpression()
    {
        // Arrange
        var specification = new NameEqualsSpecification("Test");

        // Act
        System.Linq.Expressions.Expression<Func<TestEntity, bool>> expression = specification;
        var compiledExpression = expression.Compile();

        // Assert
        compiledExpression(new TestEntity { Name = "Test" }).Should().BeTrue();
        compiledExpression(new TestEntity { Name = "Other" }).Should().BeFalse();
    }

    // Test classes
    public class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class NameEqualsSpecification : Specification<TestEntity>
    {
        private readonly string _name;

        public NameEqualsSpecification(string name)
        {
            _name = name;
        }

        public override System.Linq.Expressions.Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Name == _name;
        }
    }

    public class NameStartsWithSpecification : Specification<TestEntity>
    {
        private readonly string _prefix;

        public NameStartsWithSpecification(string prefix)
        {
            _prefix = prefix;
        }

        public override System.Linq.Expressions.Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Name.StartsWith(_prefix);
        }
    }

    public class AgeLessThanSpecification : Specification<TestEntity>
    {
        private readonly int _age;

        public AgeLessThanSpecification(int age)
        {
            _age = age;
        }

        public override System.Linq.Expressions.Expression<Func<TestEntity, bool>> ToExpression()
        {
            return entity => entity.Age < _age;
        }
    }
}
