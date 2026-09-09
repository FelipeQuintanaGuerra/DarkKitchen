namespace DomainTests;

using Domain;
using Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class UserTests
{
    private const string ValidName = "Juan";
    private const string ValidSurname = "Garcia";
    private const string ValidEmail = "juan@mail.com";
    private const string ValidPhone = "099123456";
    private const string ValidPassword = "MiPassword@Ab789";

    [TestMethod]
    public void CreateUser_WhenNameIsEmpty_ShouldThrowException()
    {
        string emptyName = "";

        Assert.ThrowsException<ArgumentException>(() =>
            new User(emptyName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenSurnameIsTooShort_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, "AB", ValidEmail, ValidPhone, ValidPassword, Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenSurnameIsTooLong_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, "EsteApellidoEsDemasiadoLargoParaSerValido", ValidEmail, ValidPhone, ValidPassword, Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenEmailIsInvalid_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, "notanemail", ValidPhone, ValidPassword, Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPhoneIsInvalid_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, "12345", ValidPassword, Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordIsTooShort_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "Short@1", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordIsTooLong_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "ThisPasswordIsWayTooLong@123456", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordHasNoUppercase_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "nouppercase@12345", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordHasNoLowercase_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "NOLOWERCASE@12345", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordHasNoSymbol_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "NoSymbol12345678", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordHasNoNumber_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "NoNumber@abcdefghij", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordHasNumericSequence_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "Password@Ab12345", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_TwoUsers_ShouldHaveDifferentIds()
    {
        User user1 = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);
        User user2 = new User("Maria", "Lopez", "maria@mail.com", "099654321", ValidPassword, Role.Client);

        Assert.AreNotEqual(user1.Id, user2.Id);
    }

    [TestMethod]
    public void CreateUser_ShouldSaveRole()
    {
        User user = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Admin);

        Assert.AreEqual(Role.Admin, user.Role);
    }

    [TestMethod]
    public void CreateUser_WhenSurnameIsEmpty_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, "", ValidEmail, ValidPhone, ValidPassword, Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenEmailIsEmpty_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, "", ValidPhone, ValidPassword, Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPhoneIsEmpty_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, "", ValidPassword, Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordIsEmpty_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPasswordHasSequence7890_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "Password@Ab7890", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_WhenPhoneIsArgentinian_ShouldNotThrowException()
    {
        new User(ValidName, ValidSurname, ValidEmail, "+5491112345678", ValidPassword, Role.Client);
    }

    [TestMethod]
    public void CreateUser_WhenPhoneIsSpanish_ShouldNotThrowException()
    {
        new User(ValidName, ValidSurname, ValidEmail, "+34612345678", ValidPassword, Role.Client);
    }

    [TestMethod]
    public void CreateUser_WhenPhoneIsUruguayanLocal_ShouldNotThrowException()
    {
        new User(ValidName, ValidSurname, ValidEmail, "099123456", ValidPassword, Role.Client);
    }

    [TestMethod]
    public void CreateUser_SamePasswordGeneratesDifferentHash()
    {
        User user1 = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);
        User user2 = new User("Maria", "Lopez", "maria@mail.com", "099654321", ValidPassword, Role.Client);

        Assert.AreNotEqual(user1.Password, user2.Password);
    }

    [TestMethod]
    public void CreateUser_VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        User user = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);

        Assert.IsTrue(user.VerifyPassword(ValidPassword));
    }

    [TestMethod]
    public void CreateUser_VerifyPassword_ShouldReturnFalseForWrongPassword()
    {
        User user = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);

        Assert.IsFalse(user.VerifyPassword("WrongPassword@Ab1"));
    }

    [TestMethod]
    public void CreateUser_WhenIsInternalIsTrue_ShouldStoreIsInternal()
    {
        User user = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Admin, true);

        Assert.IsTrue(user.IsInternal);
    }

    [TestMethod]
    public void CreateUser_ShouldBeActiveByDefault()
    {
        User user = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);

        Assert.IsTrue(user.IsActive);
    }

    [TestMethod]
    public void CreateUser_WhenNameHasOneCharacter_ShouldNotThrowException()
    {
        new User("J", ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);
    }

    [TestMethod]
    public void CreateUser_WhenSurnameHasExactlyThreeCharacters_ShouldNotThrowException()
    {
        new User(ValidName, "Abc", ValidEmail, ValidPhone, ValidPassword, Role.Client);
    }

    [TestMethod]
    public void CreateUser_WhenSurnameHasExactlyTwentyFiveCharacters_ShouldNotThrowException()
    {
        new User(ValidName, "Abcdefghijklmnopqrstuvwxy", ValidEmail, ValidPhone, ValidPassword, Role.Client);
    }

    [TestMethod]
    public void CreateUser_WhenAllDataIsValid_ShouldCreateUserSuccessfully()
    {
        User user = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);

        Assert.AreEqual(ValidName, user.Name);
        Assert.AreEqual(ValidSurname, user.Surname);
        Assert.AreEqual(ValidEmail, user.Email);
        Assert.AreEqual(ValidPhone, user.Phone);
        Assert.AreEqual(Role.Client, user.Role);
        Assert.IsTrue(user.IsActive);
        Assert.IsFalse(user.IsInternal);
    }

    [TestMethod]
    public void CreateUser_WhenPasswordHasDescendingSequence_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, "Password@Ab9876", Role.Client)
        );
    }

    [TestMethod]
    public void CreateUser_ShouldHaveEmptyOrdersList()
    {
        var user = new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);

        Assert.AreEqual(0, user.Orders.Count);
    }

    [TestMethod]
    public void CreateUser_WhenRoleIsInvalid_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, (Role)99)
        );
    }

    #region UpdateTests

    private User CreateValidUser()
    {
        return new User(ValidName, ValidSurname, ValidEmail, ValidPhone, ValidPassword, Role.Client);
    }

    [TestMethod]
    public void UpdateUser_WithValidData_ShouldUpdateFields()
    {
        var user = CreateValidUser();

        user.Update("Pedro", "Martinez", "pedro@mail.com", "099999999", Role.Admin, true, true);

        Assert.AreEqual("Pedro", user.Name);
        Assert.AreEqual("Martinez", user.Surname);
        Assert.AreEqual("pedro@mail.com", user.Email);
        Assert.AreEqual("099999999", user.Phone);
        Assert.AreEqual(Role.Admin, user.Role);
        Assert.IsTrue(user.IsInternal);
        Assert.IsTrue(user.IsActive);
    }

    [TestMethod]
    public void UpdateUser_WithEmptyName_ShouldThrowException()
    {
        var user = CreateValidUser();

        Assert.ThrowsException<ArgumentException>(() =>
            user.Update("", ValidSurname, ValidEmail, ValidPhone, Role.Client, false, true)
        );
    }

    [TestMethod]
    public void UpdateUser_WithInvalidSurname_ShouldThrowException()
    {
        var user = CreateValidUser();

        Assert.ThrowsException<ArgumentException>(() =>
            user.Update(ValidName, "AB", ValidEmail, ValidPhone, Role.Client, false, true)
        );
    }

    [TestMethod]
    public void UpdateUser_WithInvalidEmail_ShouldThrowException()
    {
        var user = CreateValidUser();

        Assert.ThrowsException<ArgumentException>(() =>
            user.Update(ValidName, ValidSurname, "notanemail", ValidPhone, Role.Client, false, true)
        );
    }

    [TestMethod]
    public void UpdateUser_WithInvalidPhone_ShouldThrowException()
    {
        var user = CreateValidUser();

        Assert.ThrowsException<ArgumentException>(() =>
            user.Update(ValidName, ValidSurname, ValidEmail, "123", Role.Client, false, true)
        );
    }

    [TestMethod]
    public void UpdateUser_WithInvalidRole_ShouldThrowException()
    {
        var user = CreateValidUser();

        Assert.ThrowsException<ArgumentException>(() =>
            user.Update(ValidName, ValidSurname, ValidEmail, ValidPhone, (Role)99, false, true)
        );
    }

    #endregion

}