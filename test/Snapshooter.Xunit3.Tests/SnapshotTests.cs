using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Snapshooter.Exceptions;
using Snapshooter.Tests.Data;
using Snapshooter.Xunit3.Tests.Helpers;
using Xunit;
using Xunit.Sdk;

namespace Snapshooter.Xunit3.Tests;

public partial class SnapshotTests
{
    #region Match Snapshot - Simple Snapshot Tests

    [Fact]
    public void Match_FactMatchSingleSnapshot_GoodCase()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        // act & assert
        Snapshot.Match(testPerson);
    }

    [Fact]
    public void Match_FactMatchSingleSnapshot_OneFieldNotEqual()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().WithAge(5).Build();

        // act
        Action match = () => Snapshot.Match(testPerson);

        // assert
        Assert.Throws<EqualException>(match);
    }

    [Fact]
    public void Match_FactMatchSingleSnapshot_FieldNotExistInSnapshot()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        // act
        Action match = () => Snapshot.Match(testPerson);

        // assert
        Assert.Throws<EqualException>(match);
    }

    [Fact]
    public void Match_FactMatchNewSingleSnapshot_ExpectedSnapshotHasBeenCreated()
    {
        // arrange
        var snapshotFullNameResolver = new SnapshotFullNameResolver(
            new Xunit3SnapshotFullNameReader());

        SnapshotFullName snapshotFullName =
            snapshotFullNameResolver.ResolveSnapshotFullName();

        string snapshotFileName = Path.Combine(
            snapshotFullName.FolderPath,
            FileNames.SnapshotFolderName,
            snapshotFullName.Filename);

        File.Delete(snapshotFileName);

        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        // act
        Snapshot.Match(testPerson);

        // assert
        Assert.True(File.Exists(snapshotFileName));
    }

    [Theory]
    [InlineData(36, 189.45)]
    [InlineData(42, 173.16)]
    [InlineData(19, 193.02)]
    public void Match_TheoryMatchSingleSnapshot_GoodCase(int age, decimal size)
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton()
            .WithAge(age).WithSize(size).Build();

        // act & assert
        Snapshot.Match(testPerson, SnapshotNameExtension.Create(age, size));
    }

    [Theory]
    [InlineData(34, 175)]
    [InlineData(36, 177)]
    [InlineData(37, 178)]
    public void Match_TheoryMatchSingleSnapshot_OneFieldNotEqual(int age, decimal size)
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton()
            .WithAge(age).WithSize(size).Build();

        testPerson.Address.Country.CountryCode = CountryCode.US;

        // act & assert
        Assert.Throws<EqualException>(() => Snapshot.Match(
            testPerson, SnapshotNameExtension.Create(age, size)));
    }

    [Theory]
    [InlineData(22, 160)]
    [InlineData(23, 164)]
    public void Match_TheoryMatchSingleSnapshot_FieldNotExistInSnapshot(int age, decimal size)
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton()
            .WithAge(age).WithSize(size).Build();

        // act & assert
        Assert.Throws<EqualException>(() => Snapshot.Match(
            testPerson, SnapshotNameExtension.Create(age, size)));
    }

    [Theory]
    [InlineData(19, 162.3)]
    [InlineData(17, 112.3)]
    public void Match_TheoryMatchNewSingleSnapshot_ExpectedSnapshotHasBeenCreated(int age, decimal size)
    {
        // arrange
        var snapshotFullNameResolver = new SnapshotFullNameResolver(
            new Xunit3SnapshotFullNameReader());

        SnapshotFullName snapshotFullName =
            snapshotFullNameResolver.ResolveSnapshotFullName();

        string snapshotFileName = Path.Combine(
            snapshotFullName.FolderPath,
            FileNames.SnapshotFolderName,
            snapshotFullName.Filename);

        File.Delete(snapshotFileName);

        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton()
            .WithAge(age).WithSize(size).Build();

        // act
        Snapshot.Match(testPerson);

        // assert
        Assert.True(File.Exists(snapshotFileName));
    }

    #endregion

    #region Match Snapshot - Multiple Objects Tests

    [Fact]
    public void Match_MultipleObjectsSnapshot_SuccessfulMatch()
    {
        // arrange
        TestPerson markWalton = TestDataBuilder.TestPersonMarkWalton().Build();
        TestPerson sandraSchneider = TestDataBuilder.TestPersonSandraSchneider().Build();
        TestChild hanna = TestDataBuilder.TestChildHanna().Build();

        // act & assert
        Snapshot.Match(new { markWalton, sandraSchneider, hanna });
    }

    [Fact]
    public void Match_ObjectsArraySnapshot_SuccessfulMatch()
    {
        // arrange
        TestPerson markWalton = TestDataBuilder.TestPersonMarkWalton().Build();
        TestPerson sandraSchneider = TestDataBuilder.TestPersonSandraSchneider().Build();
        TestChild hanna = TestDataBuilder.TestChildHanna().Build();

        // act & assert
        Snapshot.Match(new object[] { markWalton, sandraSchneider, hanna });
    }

    [Fact]
    public void Match_ObjectsListsSnapshot_SuccessfulMatch()
    {
        // arrange
        TestPerson markWalton = TestDataBuilder.TestPersonMarkWalton().Build();
        TestPerson sandraSchneider = TestDataBuilder.TestPersonSandraSchneider().Build();
        TestChild hanna = TestDataBuilder.TestChildHanna().Build();

        // act & assert
        Snapshot.Match(new List<object>() { markWalton, sandraSchneider, hanna });
    }

    #endregion

    #region Match Snapshots - Ignore Fields Tests

    [Fact]
    public void Match_IgnoreScalarField_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonSandraSchneider()
            .WithSize(1.5m)
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField("Size"));
    }

    [Fact]
    public void Match_IgnoreScalarFields_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonSandraSchneider()
            .WithSize(1.5m)
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreFields("Size"));
    }

    [Fact]
    public void Match_IgnoreScalarNullIntField_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonSandraSchneider()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField<int?>("Age"));
    }

    [Fact]
    public void Match_IgnoreScalarNullStringField_SuccessfulIgnored()
    {
        // arrange
        TestChild testChild = TestDataBuilder
            .TestChildNull()
            .Build();

        // act & assert
        Snapshot.Match(
            testChild, matchOptions => matchOptions.IgnoreField<string>("Name"));
    }

    [Fact]
    public void Match_IgnoreScalarFieldNullConvertError_ThrowsSnapshotFieldException()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonSandraSchneider()
            .Build();

        // act & assert
        Assert.Throws<SnapshotFieldException>(() => Snapshot
            .Match(testPerson, matchOptions => matchOptions.IgnoreField<int>("Age")));
    }

    [Fact]
    public void Match_IgnoreScalarFieldPathNotExist_SnapshotComparedWithoutIgnoredField()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(testPerson, matchOptions => matchOptions.IgnoreField<decimal>("alt"));
    }

    [Fact]
    public void Match_IgnoreComplexObjectField_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        testPerson.Address = null;

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField<object>("Address"));
    }

    [Fact]
    public void Match_IgnoreScalarFieldInAllWays_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonSandraSchneider()
            .WithSize(1.5m)
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField("Size"));
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField<decimal>("Size"));
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.Ignore(option => option.Field<decimal>("Size")));
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreFields("Size"));
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreFields<decimal>("Size"));
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.Ignore(option => option.Fields<decimal>("Size")));

        Assert.Throws<EqualException>(() => Snapshot.Match(testPerson));
    }

    [Fact]
    public void Match_IgnoreSeveralSingleFields_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton()
            .AddChild(TestDataBuilder.TestChildJames().Build())
            .Build();

        testPerson.Id = Guid.NewGuid();
        testPerson.CreationDate = DateTime.UtcNow;
        testPerson.Address.StreetNumber = -58;
        testPerson.Address.Country = null;
        testPerson.Relatives[0].Address.Plz = null;

        // act & assert
        Snapshot.Match(testPerson,
            matchOptions => matchOptions
                .IgnoreField<Guid>("Id")
                .IgnoreField<DateTime>("CreationDate")
                .IgnoreField<int>("Address.StreetNumber")
                .IgnoreField<TestChild>("Children[3]")
                .IgnoreField<TestCountry>("Address.Country")
                .IgnoreField<TestCountry>("Relatives[0].Address.Plz"));
    }

    [Fact]
    public void Match_IgnoreWildcardScalarFieldsArray_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton()
            .AddChild(TestDataBuilder.TestChildJames().Build())
            .Build();

        testPerson.Children.ElementAt(0).Name = "newName1";
        testPerson.Children.ElementAt(1).Name = "newName2";
        testPerson.Children.ElementAt(2).Name = "newName3";

        // act & assert
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.IgnoreFields("Children[*].Name"));
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.IgnoreFields<string>("Children[*].Name"));
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.Ignore(option => option.Fields<string>("Children[*].Name")));
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.IgnoreField("Children[*].Name"));
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.IgnoreField<string>("Children[*].Name"));

        Assert.Throws<EqualException>(() => Snapshot.Match(testPerson));
    }

    [Fact]
    public void Match_IgnoreWildcardComplexFieldsArray_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton()
            .AddChild(TestDataBuilder.TestChildJames().Build())
            .Build();

        testPerson.Children.ElementAt(0).Name = "newName1x";
        testPerson.Children.ElementAt(1).Name = "newName2x";
        testPerson.Children.ElementAt(2).Name = "newName3x";

        // act & assert
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.IgnoreFields("Children[*]"));
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.IgnoreFields<TestChild>("Children[*]"));
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.Ignore(option => option.Fields<TestChild>("Children[*]")));
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.IgnoreField("Children"));
        Snapshot.Match(testPerson,
            matchOptions => matchOptions.IgnoreField<TestChild>("Children[*]"));
    }

    [Fact]
    public void Match_IgnoreArrayFields_SuccessfulIgnored()
    {
        // arrange
        object[] testPersons = new object[]
        {
            TestDataBuilder.TestPersonMarkWalton().Build(),
            TestDataBuilder.TestPersonSandraSchneider().Build(),
            TestDataBuilder.TestPersonMarkWalton().Build(),
            TestDataBuilder.TestChildJames().Build(),
            TestDataBuilder.TestChildHanna().Build(),
            TestDataBuilder.TestCountrySwitzerland().Build()
        };

        // act & assert
        Snapshot.Match(
            testPersons, matchOptions => matchOptions.IgnoreFields<object>("[*]"));
        Snapshot.Match(
            testPersons, matchOptions => matchOptions.IgnoreField<object>("[*]"));
    }

    [Fact]
    public void Match_IgnoreArrayFieldsPersonFirstname_SuccessfulIgnored()
    {
        // arrange
        object[] testPersons = new object[]
        {
            TestDataBuilder.TestPersonMarkWalton().Build(),
            TestDataBuilder.TestPersonSandraSchneider().Build(),
            TestDataBuilder.TestPersonMarkWalton().Build()
        };

        // act & assert
        Snapshot.Match(
            testPersons, matchOptions => matchOptions.IgnoreFields<object>("[*].Firstname"));
        Snapshot.Match(
            testPersons, matchOptions => matchOptions.IgnoreField<object>("[*].Firstname"));
    }

    [Fact]
    public void Match_IgnoreArrayFieldPersonFirstname_SuccessfulIgnored()
    {
        // arrange
        object[] testPersons = new object[]
        {
            TestDataBuilder.TestPersonMarkWalton().Build(),
            TestDataBuilder.TestPersonSandraSchneider().Build(),
            TestDataBuilder.TestPersonMarkWalton().Build()
        };

        // act & assert
        Snapshot.Match(
            testPersons, matchOptions => matchOptions.IgnoreField<object>("[*].Firstname"));
    }

    [Fact]
    public void Match_IgnoreFieldFailsWithinFirstSnapshotCreation_ThrowsSnapshotFieldException()
    {
        // arrange
        var snapshotFullNameResolver = new SnapshotFullNameResolver(
            new Xunit3SnapshotFullNameReader());

        SnapshotFullName snapshotFullName =
            snapshotFullNameResolver.ResolveSnapshotFullName();

        string snapshotFileName = Path.Combine(
            snapshotFullName.FolderPath,
            FileNames.SnapshotFolderName,
            snapshotFullName.Filename);

        File.Delete(snapshotFileName);

        Environment.SetEnvironmentVariable("SNAPSHOOTER_STRICT_MODE", false.ToString());

        TestPerson testPerson = TestDataBuilder.TestPersonSandraSchneider()
            .WithSize(0.5m).Build();

        // act
        Action action = () => Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField<int>("Size"));

        // assert
        Assert.Throws<SnapshotFieldException>(action);
        Assert.False(File.Exists(snapshotFileName));
    }

    [Fact]
    public void Match_IgnoreFieldNewSingleSnapshot_ExpectedSnapshotHasBeenCreated()
    {
        // arrange
        var snapshotFullNameResolver = new SnapshotFullNameResolver(
            new Xunit3SnapshotFullNameReader());

        SnapshotFullName snapshotFullName =
            snapshotFullNameResolver.ResolveSnapshotFullName();

        string snapshotFileName = Path.Combine(
            snapshotFullName.FolderPath,
            FileNames.SnapshotFolderName,
            snapshotFullName.Filename);

        File.Delete(snapshotFileName);

        TestPerson testPerson = TestDataBuilder
            .TestPersonSandraSchneider()
            .WithSize(1.5m)
            .Build();

        // act 
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField("Size"));

        // assert
        Assert.True(File.Exists(snapshotFileName));
    }

    #endregion

    #region Match Snapshots - Ignore All Fields Tests

    [Fact]
    public void Match_IgnoreAllDateOfBirthFields_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreAllFields("DateOfBirth"));
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthFieldsByWildcard_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField("**.DateOfBirth"));
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreFields("**.DateOfBirth"));
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthFields_SuccessfulIgnoredAndTypeChecked()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreAllFields<DateTime>("DateOfBirth"));
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthFieldsByWildcard_SuccessfulIgnoredAndTypeChecked()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreField<DateTime>("**.DateOfBirth"));
        Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreFields<DateTime>("**.DateOfBirth"));
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthFields_ThrowsSnapshotException()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act
        Action match = () => Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreAllFields("DateOfBirth"));

        // assert
        EqualException exception = Assert.Throws<EqualException>(match);
        Assert.Contains("2019-04-01T00:00:01", exception.Message);
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthFields_ThrowsWrongFieldTypeException()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act
        Action match = () => Snapshot.Match(
            testPerson, matchOptions => matchOptions.IgnoreAllFields<decimal>("DateOfBirth"));

        // assert
        SnapshotFieldException exception = Assert.Throws<SnapshotFieldException>(match);
        Assert.Contains("DateOfBirth", exception.Message);
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthAndIdFields_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions
                .IgnoreAllFields("DateOfBirth")
                .IgnoreAllFields("Id"));
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthAndIdFields_SuccessfulIgnoredAndTypeChecked()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions
                .IgnoreAllFields<DateTime>("DateOfBirth")
                .IgnoreAllFields<Guid>("Id"));
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthAndIdFields_ThrowsEqualException()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Action match = () => Snapshot.Match(
            testPerson, matchOptions => matchOptions
                .IgnoreAllFields("DateOfBirth")
                .IgnoreAllFields("Id"));

        // assert
        EqualException exception = Assert.Throws<EqualException>(match);
        Assert.Contains("8001", exception.Message);
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthAndIdFields_ThrowsWrongFieldTypeException()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act
        Action match = () => Snapshot.Match(
            testPerson, matchOptions => matchOptions
                .IgnoreAllFields<DateTime>("DateOfBirth")
                .IgnoreAllFields<DateTime>("Id"));

        // assert
        SnapshotFieldException exception = Assert.Throws<SnapshotFieldException>(match);
        Assert.Contains("Id", exception.Message);
    }

    [Fact]
    public void Match_IgnoreAllDateOfBirthFieldsOfAnArray_SuccessfulIgnored()
    {
        // arrange
        object[] testPersons = new object[]
        {
            TestDataBuilder.TestPersonMarkWalton().Build(),
            TestDataBuilder.TestPersonSandraSchneider().Build(),
            TestDataBuilder.TestPersonMarkWalton().Build(),
            TestDataBuilder.TestChildJames().Build(),
            TestDataBuilder.TestChildHanna().Build(),
            TestDataBuilder.TestCountrySwitzerland().Build()
        };

        // act & assert
        Snapshot.Match(
            testPersons, matchOptions => matchOptions
            .IgnoreAllFields("CreationDate"));
    }

    [Fact]
    public void Match_IgnoreAllNotExistingFields_SuccessfulIgnored()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions
            .IgnoreAllFields("NotExistingField"));
    }

    [Fact]
    public void Match_IgnoreAllNotExistingFields_NoIgnoreNoTypeCheck()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder
            .TestPersonMarkWalton()
            .Build();

        // act & assert
        Snapshot.Match(
            testPerson, matchOptions => matchOptions
            .IgnoreAllFields<DateTime>("NotExistingField"));
    }

    #endregion

    #region Match Snapshots - Complex Tests

    [Fact]
    public void Match_LargeOverallTest_SuccessfulMatch()
    {
        // arrange
        TestChild testChild = TestDataBuilder.TestChildJames().Build();
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton()
            .AddChild(testChild)
            .Build();

        testPerson.Id = Guid.NewGuid();
        testPerson.CreationDate = DateTime.UtcNow;
        testPerson.Address.StreetNumber = -58;
        testPerson.Address.Country = null;
        testPerson.Relatives[0].Address.Plz = null;

        // act & assert
        Snapshot.Match(testPerson,
            matchOption => matchOption
    .Assert(option => Assert.NotEqual(Guid.Empty, option.Field<Guid>("Id")))
                .IgnoreField<DateTime>("CreationDate")
                .Assert(option => Assert.Equal(-58, option.Field<int>("Address.StreetNumber")))
                .Assert(option => testChild.Should().BeEquivalentTo(option.Field<TestChild>("Children[3]")))
                .IgnoreField<TestCountry>("Address.Country")
                .Assert(option => Assert.Null(option.Field<TestCountry>("Relatives[0].Address.Plz"))));
    }

    [Fact]
    public void Match_CircularReference_SuccessfulMatch()
    {
        // arrange
        TestPerson markWalton = TestDataBuilder.TestPersonMarkWalton()
            .Build();


        TestPerson sandraSchneider = TestDataBuilder.TestPersonSandraSchneider()
            .AddRelative(markWalton)
            .Build();

        markWalton.Relatives = new[] { sandraSchneider };

        // act & assert
        Snapshot.Match(markWalton);
    }

    #endregion

    #region Match Snapshots - Scalar Types Tests

    [Fact]
    public void Match_FactMatchScalarStringValueSnapshot_SuccessfulMatch()
    {
        // arrange
        string testText = "This is a test string for the " +
            "snapshot test with a plain string value";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchScalarStringValueSnapshot_ChangedLetter()
    {
        // arrange
        string testText = "This is a fest string for the " +
            "snapshot test with a plain string value";

        // act
        Action match = () => Snapshot.Match(testText);

        // assert
        Assert.Contains("fest",
            Assert.Throws<EqualException>(match).Message);
    }

    [Fact]
    public void Match_FactMatchScalarStringValueSnapshot_IgnoreOptionFails()
    {
        // arrange
        string testText = "This is a test string for the " +
            "snapshot test with a plain string value";

        // act
        Action match = () => Snapshot.Match(
            testText, matchOption => matchOption.IgnoreField("test"));

        // assert
        Assert.Contains("field",
            Assert.Throws<SnapshotFieldException>(match).Message);
    }

    [Fact]
    public void Match_FactMatchScalarEmptyCommentsStringValueSnapshot_SuccessfulMatch()
    {
        // arrange
        string testText = "/**/";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchScalarEmptyCommentsStringValueSnapshot_ChangedInput()
    {
        // arrange
        string testText = "/*";

        // act
        Action match = () => Snapshot.Match(testText);

        // assert
        Assert.Contains("*/",
            Assert.Throws<EqualException>(match).Message);
    }

    [Fact]
    public void Match_FactMatchScalarCommentsStringValueSnapshot_SuccessfulMatch()
    {
        // arrange
        string testText = "/*This is a comment string*/";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchScalarCommentsStringValueSnapshot_ChangedInput()
    {
        // arrange
        string testText = "/*This is a fest comment string for the" +
            "snapshot test with a plain string value */";

        // act
        Action match = () => Snapshot.Match(testText);

        // assert
        Assert.Contains("fest",
            Assert.Throws<EqualException>(match).Message);
    }

    [Fact]
    public void Match_FactMatchScalarIntegerValueSnapshot_SuccessfulMatch()
    {
        // arrange
        int testNumber = 5;

        // act & assert
        Snapshot.Match(testNumber);
    }

    [Fact]
    public void Match_FactMatchScalarIntegerValueSnapshot_ChangedNumberNotEqual()
    {
        // arrange
        int testNumber = 5;

        // act
        Action match = () => Snapshot.Match(testNumber);

        // assert
        Assert.Contains("6",
            Assert.Throws<EqualException>(match).Message);
    }

    [Fact]
    public void Match_FactMatchScalarIntegerValueSnapshot_IgnoreOptionFails()
    {
        // arrange
        int testNumber = 5;

        // act
        Action match = () => Snapshot.Match(
            testNumber, matchOption => matchOption.IgnoreField("6"));

        // assert
        Assert.Contains("6",
            Assert.Throws<SnapshotFieldException>(match).Message);
    }

    #endregion

    #region Match Snapshots - Stream Types Tests

    [Fact]
    public void Match_FactMatchMemoryStreamSnapshot_SuccessfulMatch()
    {
        // arrange
        MemoryStream testMemoryStream =
            new MemoryStream(Encoding.ASCII.GetBytes("Foo Bar 35"));

        // act & assert
        Snapshot.Match(testMemoryStream);
    }

    [Fact]
    public void Match_FactMatchObjectWithMemoryStreamSnapshot_SuccessfulMatch()
    {
        // arrange
        var testUser = new
        {
            FirstName = "Foo",
            Age = 35,
            Picture = new MemoryStream(Encoding.ASCII.GetBytes("Foo Bar 35"))
        };

        // act & assert
        Snapshot.Match(testUser);
    }

    [Fact]
    public void Match_FactMatchStreamSnapshot_SuccessfulMatch()
    {
        // arrange
        Stream testStream =
            TestFileLoader.LoadFileStream("mona-lisa.jpg");

        // act & assert
        Snapshot.Match(testStream);
    }

    [Fact]
    public void Match_FactMatchObjectWithStreamSnapshot_SuccessfulMatch()
    {
        // arrange
        var testUser = new
        {
            FirstName = "Foo",
            Age = 35,
            Picture = TestFileLoader.LoadFileStream("mona-lisa.jpg")
        };

        // act & assert
        Snapshot.Match(testUser);
    }

    [Fact]
    public void Match_FactMatchFileStreamSnapshot_SuccessfulMatch()
    {
        // arrange
        string folderPath = SnapshotDefaultNameResolver
            .ResolveSnapshotDefaultFullName()
            .FolderPath;

        Stream testStream = 
            File.OpenRead($"{folderPath}/__testsources__/mona-lisa.jpg");

        // act & assert
        Snapshot.Match(testStream);
    }

    [Fact]
    public void Match_FactMatchObjectWithFileStreamSnapshot_SuccessfulMatch()
    {
        // arrange
        string folderPath = SnapshotDefaultNameResolver
            .ResolveSnapshotDefaultFullName()
            .FolderPath;

        var testUser = new
        {
            FirstName = "Foo",
            Age = 35,
            Picture = File.OpenRead($"{folderPath}/__testsources__/mona-lisa.jpg")
        };

        // act & assert
        Snapshot.Match(testUser);
    }

    [Fact]
    public void Match_FactMatchObjectWithAllStreamsSnapshot_SuccessfulMatch()
    {
        // arrange
        string folderPath = SnapshotDefaultNameResolver
            .ResolveSnapshotDefaultFullName()
            .FolderPath;

        var testUser = new
        {
            FirstName = "Foo",
            Age = 35,
            MemoryStreamName = new MemoryStream(Encoding.ASCII.GetBytes("Foo Bar 35")),
            EmbeddedStreamPicture = TestFileLoader.LoadFileStream("mona-lisa.jpg"),
            FileStreamPicture = File.OpenRead($"{folderPath}/__testsources__/mona-lisa.jpg")
        };

        // act & assert
        Snapshot.Match(testUser);
    }

    #endregion

    #region Match Snapshots - Crlf Tests

    [Fact]
    public void Match_FactMatchSnapshotWithCrlfString_SuccessfulMatch()
    {
        // arrange
        string testText = "query fetch {\r\n  customer(id: \"Q3VzdG9tZXIteDE=\") {\r\n    " +
            "name\r\n    consultant {\r\n      name\r\n      __typename\r\n    " +
            "}\r\n    id\r\n    __typename\r\n  }\r\n}";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCrlfStringFormatted_SuccessfulMatch()
    {
        // arrange
        string testText = "query fetch {\r\n  customer(id: \"Q3VzdG9tZXIteDE=\") {\r\n    " +
            "name\r\n    consultant {\r\n      name\r\n      __typename\r\n    " +
            "}\r\n    id\r\n    __typename\r\n  }\r\n}";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithWrongCrlfStringFormatted_ThrowsSnapshotCompareException()
    {
        // arrange
        string testText = "query fetch {\r\n  customer(id: \"Q3VzdG9tZXIteDE=\") {\r\n    " +
            "name\r\n    consultant {\r\n      name\r\n      __typename\r\n    " +
            "}\r\n    id\r\n    __typename\r\n  }\r\n}";

        // act
        Action match = () => Snapshot.Match(testText);

        // assert
        Assert.Throws<EqualException>(match);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithLfString_SuccessfulMatch()
    {
        // arrange
        string testText = "query fetch {\n  customer(id: \"Q3VzdG9tZXIteDE=\") {\n    " +
            "name\n    consultant {\n      name\n      __typename\n    " +
            "}\n    id\n    __typename\n  }\n}";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCrString_SuccessfulMatch()
    {
        // arrange
        string testText = "query fetch {\r  customer(id: \"Q3VzdG9tZXIteDE=\") {\r    " +
            "name\r    consultant {\r      name\r      __typename\r    " +
            "}\r    id\r    __typename\r  }\r}";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCommentedCrlfString_SuccessfulMatch()
    {
        // arrange
        string testText = "query fetch {\r\n  customer(id: \"Q3VzdG9tZXIteDE= \\r\\n \") {\r\n    " +
            "name\r\n    consultant {\r\n      name\\r\\n\r\n    __typename\r\n    " +
            "}\r\n    id\r\n    __typename\r\n  }\r\n}";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCommentedLfString_SuccessfulMatch()
    {
        // arrange
        string testText = "query fetch {\n  customer(id: \"Q3VzdG9tZXIteDE= \\r\\n \") {\n    " +
            "name\n    consultant {\n      name\\r\\n\n    __typename\n    " +
            "}\n    id\n    __typename\n  }\n}";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCommentedCrString_SuccessfulMatch()
    {
        // arrange
        string testText = "query fetch {\r  customer(id: \"Q3VzdG9tZXIteDE= \\r \") {\r    " +
            "name\r    consultant {\r      name\\r\r    __typename\r    " +
            "}\r    id\r    __typename\r  }\r}";

        // act & assert
        Snapshot.Match(testText);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCrLfStringInObject_SuccessfulMatch()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        testPerson.Address.City = "\r\n test \n city \r";
        testPerson.Lastname = "Your\r\nName\nAt\rHome";

        // act & assert
        Snapshot.Match(testPerson);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCrLfStringInFile_ThrowsException()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        testPerson.Address.City = "\r\n test \n city \r";
        testPerson.Lastname = "Your\r\nName\nAt\rHome";

        // act & assert
        Action match = () => Snapshot.Match(testPerson);

        // assert
        Assert.Throws<EqualException>(match);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithMissingCrlfStringWithinObject_ThrowsException()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        testPerson.Lastname = "last \r\n name \r with \r\n carriage return";

        // act
        Action match = () => Snapshot.Match(testPerson);

        // assert
        Assert.Throws<EqualException>(match);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCommentedCrLfStringInObject_SuccessfulMatch()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        testPerson.Lastname = "\\r\\n last \r\n name \r with \r\n carriage \\r\\n return \\r\\n";

        // act & assert
        Snapshot.Match(testPerson);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCommentedCrStringInObject_SuccessfulMatch()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        testPerson.Lastname = "\\r last \r\n name \r with \r\n carriage \\r return \\r";

        // act & assert
        Snapshot.Match(testPerson);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCommentedLfStringInObject_SuccessfulMatch()
    {
        // arrange
        TestPerson testPerson = TestDataBuilder.TestPersonMarkWalton().Build();

        testPerson.Lastname = "\\n last \r\n name \r with \r\n carriage \\n return \\n";

        // act & assert
        Snapshot.Match(testPerson);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCrLfStringJsonWithinComplexObject_SuccessfulMatch()
    {
        // arrange
        var testMessageInfo = new TestMessageInfo()
        {
            Name = "Name of the test message",
            Content = "{\r\n  \"Id\": \"c78c698f-9ee5-4b4b-9a0e-ef729b1f8ec8\",\r\n  \"Firstname\": \"Mark\",\r\n  \"Lastname\": \"last \\r\\n name \\r with \r\n carriage return\",\r\n  \"CreationDate\": \"2018-06-06T00:00:00\",\r\n  \"DateOfBirth\": \"2000-06-25T00:00:00\",\r\n  \"Age\": 30,\r\n  \"Size\": 182.5214,\r\n  \"Address\": {\r\n    \"Street\": \"Rohrstrasse\",\r\n    \"StreetNumber\": 12,\r\n    \"Plz\": 8304,\r\n    \"City\": \"Wallislellen\",\r\n    \"Country\": {\r\n      \"Name\": \"Switzerland\",\r\n      \"CountryCode\": \"CH\"\r\n    }\r\n  },\r\n  \"Children\": [\r\n    {\r\n      \"Name\": \"\\r\\nJames\r\n\",\r\n      \"DateOfBirth\": \"2015-02-12T00:00:00\"\r\n    },\r\n    {\r\n      \"Name\": null,\r\n      \"DateOfBirth\": \"2015-02-12T00:00:00\"\r\n    },\r\n    {\r\n      \"Name\": \"Hanna\",\r\n      \"DateOfBirth\": \"2012-03-20T00:00:00\"\r\n    }\r\n  ],\r\n  \"Relatives\": [\r\n    {\r\n      \"Id\": \"fcf04ca6-d8f2-4214-a3ff-d0ded5bad4de\",\r\n      \"Firstname\": \"Sandra\",\r\n      \"Lastname\": \"Schneider\",\r\n      \"CreationDate\": \"2019-04-01T00:00:00\",\r\n      \"DateOfBirth\": \"1996-02-14T00:00:00\",\r\n      \"Age\": null,\r\n      \"Size\": 165.23,\r\n      \"Address\": {\r\n        \"Street\": \"Bahnhofstrasse\",\r\n        \"StreetNumber\": 450,\r\n        \"Plz\": 8000,\r\n        \"City\": \"Zurich\",\r\n        \"Country\": {\r\n          \"Name\": \"Switzerland\",\r\n          \"CountryCode\": \"CH\"\r\n        }\r\n      },\r\n      \"Children\": [],\r\n      \"Relatives\": null\r\n    }\r\n  ]\r\n}",
            Error = new Exception("Error Titel: \r\n Remove Carriage Returns")
        };

        // act & assert
        Snapshot.Match(testMessageInfo);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCrStringJsonWithinComplexObject_SuccessfulMatch()
    {
        // arrange
        var testMessageInfo = new TestMessageInfo()
        {
            Name = "Name of the test message",
            Content = "{\r  \"Id\": \"c78c698f-9ee5-4b4b-9a0e-ef729b1f8ec8\",\r  \"Firstname\": \"Mark\",\r  \"Lastname\": \"last \\r name \r with \\r carriage return\",\r  \"CreationDate\": \"2018-06-06T00:00:00\",\r  \"DateOfBirth\": \"2000-06-25T00:00:00\",\r  \"Age\": 30,\r  \"Size\": 182.5214,\r  \"Address\": {\r    \"Street\": \"Rohrstrasse\",\r    \"StreetNumber\": 12,\r    \"Plz\": 8304,\r    \"City\": \"Wallislellen\",\r    \"Country\": {\r      \"Name\": \"Switzerland\",\r      \"CountryCode\": \"CH\"\r    }\r  },\r  \"Children\": [\r    {\r      \"Name\": \"\\rJames\r\",\r      \"DateOfBirth\": \"2015-02-12T00:00:00\"\r    },\r    {\r      \"Name\": null,\r      \"DateOfBirth\": \"2015-02-12T00:00:00\"\r    },\r    {\r      \"Name\": \"Hanna\",\r      \"DateOfBirth\": \"2012-03-20T00:00:00\"\r    }\r  ],\r  \"Relatives\": [\r    {\r      \"Id\": \"fcf04ca6-d8f2-4214-a3ff-d0ded5bad4de\",\r      \"Firstname\": \"Sandra\",\r      \"Lastname\": \"Schneider\",\r      \"CreationDate\": \"2019-04-01T00:00:00\",\r      \"DateOfBirth\": \"1996-02-14T00:00:00\",\r      \"Age\": null,\r      \"Size\": 165.23,\r      \"Address\": {\r        \"Street\": \"Bahnhofstrasse\",\r        \"StreetNumber\": 450,\r        \"Plz\": 8000,\r        \"City\": \"Zurich\",\r        \"Country\": {\r          \"Name\": \"Switzerland\",\r          \"CountryCode\": \"CH\"\r        }\r      },\r      \"Children\": [],\r      \"Relatives\": null\r    }\r  ]\r}",
            Error = new Exception("Error Titel: \r Remove Carriage Returns")
        };

        // act & assert
        Snapshot.Match(testMessageInfo);
    }

    [Fact]
    public void Match_FactMatchSnapshotWithCrLfStringJsonWithinAnonymousObject_SuccessfulMatch()
    {
        // arrange
        var testChild = new
        {
            Name = "{\r\n  \"Id\": \"c78c698f-9ee5-4b4b-9a0e-ef729b1f8ec8\",\r\n  \"Firstname\": \"Mark\",\r\n  \"Lastname\": \"last \\r\\n name \\r with \\r\\n carriage return\",\r\n  \"CreationDate\": \"2018-06-06T00:00:00\",\r\n  \"DateOfBirth\": \"2000-06-25T00:00:00\",\r\n  \"Age\": 30,\r\n  \"Size\": 182.5214,\r\n  \"Address\": {\r\n    \"Street\": \"Rohrstrasse\",\r\n    \"StreetNumber\": 12,\r\n    \"Plz\": 8304,\r\n    \"City\": \"Wallislellen\",\r\n    \"Country\": {\r\n      \"Name\": \"Switzerland\",\r\n      \"CountryCode\": \"CH\"\r\n    }\r\n  },\r\n  \"Children\": [\r\n    {\r\n      \"Name\": \"James\",\r\n      \"DateOfBirth\": \"2015-02-12T00:00:00\"\r\n    },\r\n    {\r\n      \"Name\": null,\r\n      \"DateOfBirth\": \"2015-02-12T00:00:00\"\r\n    },\r\n    {\r\n      \"Name\": \"Hanna\",\r\n      \"DateOfBirth\": \"2012-03-20T00:00:00\"\r\n    }\r\n  ],\r\n  \"Relatives\": [\r\n    {\r\n      \"Id\": \"fcf04ca6-d8f2-4214-a3ff-d0ded5bad4de\",\r\n      \"Firstname\": \"Sandra\",\r\n      \"Lastname\": \"Schneider\",\r\n      \"CreationDate\": \"2019-04-01T00:00:00\",\r\n      \"DateOfBirth\": \"1996-02-14T00:00:00\",\r\n      \"Age\": null,\r\n      \"Size\": 165.23,\r\n      \"Address\": {\r\n        \"Street\": \"Bahnhofstrasse\",\r\n        \"StreetNumber\": 450,\r\n        \"Plz\": 8000,\r\n        \"City\": \"Zurich\",\r\n        \"Country\": {\r\n          \"Name\": \"Switzerland\",\r\n          \"CountryCode\": \"CH\"\r\n        }\r\n      },\r\n      \"Children\": [],\r\n      \"Relatives\": null\r\n    }\r\n  ]\r\n}",
            DateOfBirth = DateTime.Now,
            TestException = new Exception("Test exception")
        };

        // act & assert
        Snapshot.Match(testChild, matchOptions =>
            matchOptions.IgnoreField(nameof(testChild.DateOfBirth)));
    }

    #endregion
}
