# Dhahabi.ViewModel

[![NuGet](https://img.shields.io/nuget/v/Dhahabi.ViewModel.svg)](https://www.nuget.org/packages/Dhahabi.ViewModel/)
[![NuGet](https://img.shields.io/nuget/dt/Dhahabi.ViewModel.svg)](https://www.nuget.org/packages/Dhahabi.ViewModel/)

## Descripción

**DhahabiViewModel** es un generador de código para C# que simplifica la implementación de `INotifyPropertyChanged` en
tus ViewModels. Utilizando un atributo personalizado, puedes reducir significativamente el código repetitivo al
implementar esta interfaz.

## Características

- Genera automáticamente los métodos `get` y `set` para propiedades marcadas con el atributo `[ObservablePropertyAttribute]`.
- Implementa la interfaz `INotifyPropertyChanged` para notificaciones de cambio de propiedad.
- Reduce la cantidad de código repetitivo en tus ViewModels.

## Instalación

Puedes instalar el paquete desde NuGet:

```sh
dotnet add package Dhahabi.ViewModel
```

O utilizando el administrador de paquetes NuGet en Visual Studio.

## Uso

Para utilizar **DhahabiViewModel**, sigue estos pasos:

1. Agrega el atributo `[ObservablePropertyAttribute]` a las propiedades de tu clase `ViewModel` que deseas generar automáticamente:

```csharp
using System.ComponentModel;

namespace MyNamespace
{
    public partial class MyViewModel
    {
        [ObservableProperty]
        public bool _isLoading;

        [ObservableProperty]
        private string _status;
    }
}
```

2. El generador de código se encargará de implementar automáticamente los métodos `get` y `set` para las propiedades
   marcadas con el atributo `[ObservablePropertyAttribute]`, así como la interfaz `INotifyPropertyChanged`.

## Ejemplo

### Antes de usar DhahabiViewModel

```csharp
public class MyViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    private string _status;
    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### Después de usar DhahabiViewModel

```csharp
namespace MyNamespace
{
    public partial class MyViewModel
    {
        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _status;
    }
}
```

El generador de código se encargará de implementar automáticamente la interfaz `INotifyPropertyChanged` y los métodos
`get` y `set` para las propiedades.

## Contribuciones

Si deseas contribuir al desarrollo de este paquete, ¡siéntete libre de hacerlo! Puedes clonar el repositorio, realizar
cambios y abrir un pull request.

## Licencia

Este proyecto está licenciado bajo la licencia MIT. Consulta el archivo [LICENSE](https://example.com/license) para más
detalles.
