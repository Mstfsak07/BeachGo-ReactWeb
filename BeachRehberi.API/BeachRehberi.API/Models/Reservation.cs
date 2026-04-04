public class Reservation : BaseEntity
{
    public void CollectFirstNameAndLastName()
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public void CollectEmail()
    {
        Email = email;
    }

    public void CollectPhone()
    {
        Phone = phone;
    }
}
