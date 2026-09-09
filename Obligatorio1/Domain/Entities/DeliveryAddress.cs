namespace Domain;

public class DeliveryAddress
{
    public string Street { get; private set; }
    public string DoorNumber { get; private set; }
    public string Apartment { get; private set; }

    public DeliveryAddress(string street, string doorNumber, string apartment)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new Exception("La calle es requerida.");

        if (string.IsNullOrWhiteSpace(doorNumber))
            throw new Exception("El número de puerta es requerido.");

        Street = street;
        DoorNumber = doorNumber;
        Apartment = apartment;
    }
}