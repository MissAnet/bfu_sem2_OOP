using System;
using System.Collections.Generic;

//Интерфейс для системы отслеживания изменений
public interface IPropertyChangedListener<T>
{
    //Определяет метод, который вызывается после изменения свойства в объекте типа T
    void OnPropertyChanged(T obj, string propertyName);//T - объект, в котором изменилось свойство , propertyName - имя изменённого свойства
}

//Интерфейс для уведомляющих обьектов
public interface INotifyDataChanged<T>
{
    void AddPropertyChangedListener(IPropertyChangedListener<T> listener); //для уведомления
    void RemovePropertyChangedListener(IPropertyChangedListener<T> listener); // для прекращения уведомления
}

//интерфейс для системы валидации изменений
public interface IPropertyChangingListener<T>
{
    bool OnPropertyChanging(T obj, string propertyName, object oldValue, object newValue); //T obj - где меняется свойство, propertyName - имя свойства
}

//Интерфейс для объектов с валидацией
public interface INotifyDataChanging<T>
{
    void AddPropertyChangingListener(IPropertyChangingListener<T> listener); //Проверять валидаторы
    void RemovePropertyChangingListener(IPropertyChangingListener<T> listener); //Удалять валидаторы
}

//абстрактный класс
public abstract class ObservableObject<T> : INotifyDataChanged<T>, INotifyDataChanging<T>
{
    private readonly List<IPropertyChangedListener<T>> _changedListeners = new List<IPropertyChangedListener<T>>(); //список на изменение 
    private readonly List<IPropertyChangingListener<T>> _changingListeners = new List<IPropertyChangingListener<T>>(); //список валидаторов изменений

    //Добавить на изменение св-в
    public void AddPropertyChangedListener(IPropertyChangedListener<T> listener)
    {
        if (!_changedListeners.Contains(listener))
        {
            _changedListeners.Add(listener);
        }
    }

    //удалить из списка на изменения св-в
    public void RemovePropertyChangedListener(IPropertyChangedListener<T> listener)
    {
        _changedListeners.Remove(listener);
    }

    //Добавить влидатор изменений
    public void AddPropertyChangingListener(IPropertyChangingListener<T> listener)
    {
        if (!_changingListeners.Contains(listener))
        {
            _changingListeners.Add(listener);
        }
    }

    //удалить валидатор изменений
    public void RemovePropertyChangingListener(IPropertyChangingListener<T> listener)
    {
        _changingListeners.Remove(listener);
    }


    protected bool SetProperty<TValue>(string propertyName, ref TValue field, TValue newValue)
    {
        //совпадает ли новое значение с текущим
        if (EqualityComparer<TValue>.Default.Equals(field, newValue))
            return false;

        //Вызываем валидацию изменений
        foreach (var listener in _changingListeners)
        {
            if (!listener.OnPropertyChanging((T)(object)this, propertyName, field, newValue))
            {
                Console.WriteLine($"Изменение свойства {propertyName} отклонено валидатором {listener.GetType().Name}");
                return false;
            }
        }

        field = newValue; //по указанной ссылке меняем значение на новое

        //Уведомляем об изменении
        foreach (var listener in _changedListeners)
        {
            listener.OnPropertyChanged((T)(object)this, propertyName);
        }

        return true;
    }
}

//Пример класса с отслеживаемыми свойствами
public class Person : ObservableObject<Person>
{
    private string _name;
    private int _age;
    private string _email;

    public string Name
    {
        get => _name;
        set => SetProperty(nameof(Name), ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => SetProperty(nameof(Age), ref _age, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(nameof(Email), ref _email, value);
    }

    //дефолтный
    public override string ToString()
    {
        return $"Name: {Name}, Age: {Age}, Email: {Email}";
    }
    //кастомный
    public string ToString(string format)
    {
        return format switch
        {
            "short" => $"{Name}, {Age}, {Email}",
            "long" => $"Name: {Name}\nAge: {Age}\nEmail: {Email}",
            "json" => $"{{\"name\":\"{Name}\",\"age\":{Age},\"email\":\"{Email}\"}}",
            _ => this.ToString()
        };
    }

}

//слушатель изменений
public class PersonChangeLogger : IPropertyChangedListener<Person>
{
    public void OnPropertyChanged(Person person, string propertyName)
    {
        Console.WriteLine($"[Изменение] {propertyName} = {GetPropertyValue(person, propertyName)}");
    }

    private object GetPropertyValue(Person person, string propertyName)
    {
        return propertyName switch{
            nameof(Person.Name) => person.Name,
            nameof(Person.Age) => person.Age,
            nameof(Person.Email) => person.Email
        };
    }
}

//валидатор изменений
public class AgeValidator : IPropertyChangingListener<Person>
{
    public bool OnPropertyChanging(Person person, string propertyName, object oldValue, object newValue)
    {
        if (propertyName == nameof(Person.Age))
        {
            if (newValue is int age)
            {
                if (age < 0)
                {
                    Console.WriteLine("Возраст не может быть отрицательным!");
                    return false;
                }

                if (age > 150)
                {
                    Console.WriteLine("Возраст не может быть больше 150!");
                    return false;
                }
            }
        }
        return true;
    }
}

public class EmailValidator : IPropertyChangingListener<Person>
{
    public bool OnPropertyChanging(Person person, string propertyName, object oldValue, object newValue)
    {
        if (propertyName == nameof(Person.Email))
        {
            if (newValue is string email && !string.IsNullOrEmpty(email))
            {
                //Проверяем, что email содержит ровно один символ @
                var atCount = email.Count(c => c == '@');
                if (atCount != 1)
                {
                    Console.WriteLine("Email должен содержать ровно один символ @!");
                    return false;
                }

                //парсим на части до и после @
                var parts = email.Split('@');
                var namePart = parts[0];
                var domainPart = parts[1];

                //часть до @
                if (string.IsNullOrEmpty(namePart))
                {
                    Console.WriteLine("Email должен содержать имя перед @!");
                    return false;
                }

                //часть после @
                if (!domainPart.Contains('.'))
                {
                    Console.WriteLine("Домен email должен содержать точку (.)!");
                    return false;
                }

                //проверка, что точка не в начале или конце домена
                if (domainPart.StartsWith(".") || domainPart.EndsWith("."))
                {
                    Console.WriteLine("Домен email не может начинаться или заканчиваться точкой!");
                    return false;
                }
            }
        }
        return true;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var person = new Person
        {
            Name = "Аннета Волтогар",
            Age = 20,
            Email = "anneta@mail.com"
        };

        //Добавляем слушателей изменений
        var changeLogger = new PersonChangeLogger();
        person.AddPropertyChangedListener(changeLogger);

        //Добавляем валидаторы
        var ageValidator = new AgeValidator();
        var emailValidator = new EmailValidator();
        person.AddPropertyChangingListener(ageValidator);
        person.AddPropertyChangingListener(emailValidator);

        Console.WriteLine("Демонстрация работы системы:");

        //Корректные изменения
        person.Name = "Эпик Айс";
        person.Age = 22;
        person.Email = "EpicIce@mail.com";

        person.Age = -5; 
        person.Age = 200; 
        person.Email = "invalid-email"; 

        //итоговые значения
        Console.WriteLine("\nИтоговые значения:");
        Console.WriteLine(person);
        Console.WriteLine(person.ToString("short"));
    }
}