# Ckode.ServiceLocator
Ckode.ServiceLocator is a simple natively implemented service locator for simplifying dependency injection.

## Installation:

I recommend using the NuGet package: https://www.nuget.org/packages/Ckode.ServiceLocator/ however you can also simply clone the repository and use the pre-compiled binaries or compile the project yourself.
As the project is licensed under MIT you're free to use it for pretty much anything you want.

## Examples:

*Create a single instance:*

    ISomeInterface instance = ServiceLocator.CreateInstance<ISomeInterface>(); // Requires that only a single class implements ISomeInterface


*Create multiple instances:*

    IEnumerable<ISomeInterface> instances = ServiceLocator.CreateInstances<ISomeInterface>(); // Works regardless of the number of implementations, as you get an IEnumerable of instances

*Create instance based on predicate:*

    ISomeInterface instance = ServiceLocator.CreateInstance<ISomeInterface>(inst => inst.UseThisOne(someArgument)); // Requires that only a single class implements ISomeInterface AND fulfills the predicate

*Fake dependency for e.g. unit tests:*

    public interface IDependency
    {
        string Value{ get; }
    }
    
    public class ActualImplementation : IDependency
    {
        // Some actual implementation
    }

    public class TextFormatter
    {
        public string AppendValueFromDependency(string textToAppendTo)
        {
            var dependency = ServiceLocator.CreateInstance<IDependency>();
            return textToAppendTo + dependency.Value;
        }
    }

    public class TextFormattersTests
    {
        private class MyFake : IDependency
        {
            public string Value => "World";
        }
    
        [Fact]
        public void TextFormatter_AppendValueFromDependency_GeneratesProperString()
        {
            // Arrange
            ServiceLocator.Bind<IDependency, MyFake>(); // Forces TextFormatter to use MyFake instead of whatever the ActualImplementation was doing
            var formatter = new TextFormatter();
            
            // Act
            var value = formatter.AppendValueFromDependency("Hello ");
            
            // Assert
            Assert.Equal("Hello world", value);
            
            // Cleanup
            ServiceLocator.Unbind<IDependency>(); // Optionally unbind IDependency, if you don't want it to be MyFake in other tests as the binding is static
        }
    }

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
    
    var locator = new ServiceLocator<VehicleType, IVehicle();
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
    
    var locator = new ServiceLocator<VehicleType, IVehicle>();
    IEnumerable<IVehicle> vehicles = locator.CreateInstances(); // Still requires that only a single class has each LocatorKey, but does work fine despite some keys not being implemented yet.

# Documentation
Auto generated documentation via [DocFx](https://github.com/dotnet/docfx) is available here: https://steffenskov.github.io/Ckode.ServiceLocator/