# Ckode.ServiceLocator
Ckode.ServiceLocator is a simple natively implemented service locator for simplifying dependency injection.

**Examples:**

*Create a single instance:*

    var locator = new ServiceLocator();
    ISomeInterface instance = locator.CreateInstance<ISomeInterface>(); // Requires that only a single class implements ISomeInterface


*Create multiple instances:*

    var locator = new ServiceLocator();
    IEnumerable<ISomeInterface> instances = locator.CreateInstances<ISomeInterface>(); // Works regardless of the number of implementations, as you get an IEnumerable of instances

*Create instance based on a key:*

    public enum VehicleType
    {
		Car,
		Bike,
		Plane,
		Boat
    }

    public interface IVehicle: ILocatable<VehicleType>
    {
    }
    
    public class Car: IVehicle
    {
		public VehicleType LocatorKey => VehicleType.Car;
    }
    
    var locator = new ServiceLocator();
    IVehicle car = locator.CreateInstance(VehicleType.Car); // Requires that only a single class has LocatorKey == VehicleType.Car

*Create multiple instances with keys:*


    public enum VehicleType
    {
		Car,
		Bike,
		Plane,
		Boat
    }

    public interface IVehicle: ILocatable<VehicleType>
    {
    }
    
    public class Car: IVehicle
    {
		public VehicleType LocatorKey => VehicleType.Car;
    }
    
    var locator = new ServiceLocator();
    IEnumerable<IVehicle> vehicles = locator.CreateInstances(); // Still requires that only a single class has each LocatorKey, but does work fine despite some keys not being implemented yet.
