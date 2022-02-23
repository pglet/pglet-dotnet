using System;

namespace Pglet
{
    public class SideValues<T>
    {
        T _top;
        T _right;
        T _bottom;
        T _left;

        public SideValues(T allSides)
            : this(allSides, allSides, allSides, allSides)
        {
        }

        public SideValues(T topAndBottom, T leftAndRight)
            : this(topAndBottom, leftAndRight, topAndBottom, leftAndRight)
        {
        }

        public SideValues(T top, T leftAndRight, T bottom)
            : this(top, leftAndRight, bottom, leftAndRight)
        {
        }

        public SideValues(T top, T right, T bottom, T left)
        {
            if (top == null) throw new ArgumentNullException("top");
            if (right == null) throw new ArgumentNullException("right");
            if (bottom == null) throw new ArgumentNullException("bottom");
            if (left == null) throw new ArgumentNullException("left");

            _top = top;
            _right = right;
            _bottom = bottom;
            _left = left;
        }

        public T Top
        {
            get { return _top; }
            set { _top = value; }
        }

        public T Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public T Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }

        public T Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public static implicit operator SideValues<T>(T singleValue) => new SideValues<T>(singleValue, singleValue, singleValue, singleValue);

        public static implicit operator SideValues<T>((T, T) twoValues) => new SideValues<T>(twoValues.Item1, twoValues.Item2);

        public static implicit operator SideValues<T>((T, T, T) threeValues) => new SideValues<T>(threeValues.Item1, threeValues.Item2, threeValues.Item3);

        public static implicit operator SideValues<T>((T, T, T, T) fourValues) => new SideValues<T>(fourValues.Item1, fourValues.Item2, fourValues.Item3, fourValues.Item4);

        public static SideValues<T> Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException("s");
            }

            string[] parts = s.Split(' ');
            var top = ParsePart(parts[0]);
            var right = ParsePart(parts[1]);
            var bottom = ParsePart(parts[2]);
            var left = ParsePart(parts[3]);
            return new SideValues<T>(top, right, bottom, left);
        }

        private static T ParsePart(string s)
        {
            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), s, true);
            }
            else
            {
                return (T)Convert.ChangeType(s, typeof(T));
            }
        }

        public override string ToString()
        {
            return $"{_top} {_right} {_bottom} {_left}".ToLowerInvariant();
        }
    }
}