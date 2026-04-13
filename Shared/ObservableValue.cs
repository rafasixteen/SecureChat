namespace Shared
{
    public class ObservableValue<T>(T initial)
    {
        private T _value = initial;

        public event Action<T>? ValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    ValueChanged?.Invoke(value);
                }
            }
        }
    }
}
