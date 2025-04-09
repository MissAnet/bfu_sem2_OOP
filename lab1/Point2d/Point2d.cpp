#include <iostream>
#include <cmath>
#include <stdexcept>
#include <algorithm>

const int WIDTH = 1920;
const int HEIGHT = 1080;

using namespace std;

class Point2d {
private:
    int x_;
    int y_;

    void validateCoordinates(int x, int y) {
        if (x < 0 || x > WIDTH || y < 0 || y > HEIGHT) {
            throw out_of_range("Координаты за пределами экрана");
        }
    }

public:
    Point2d(int x, int y) : x_(x), y_(y) {
        validateCoordinates(x, y);
    }

    // сет
    int x() const { return x_; }
    int y() const { return y_; }

    void set_x(int x) {
        validateCoordinates(x, y_);
        x_ = x;
    }

    void set_y(int y) {
        validateCoordinates(x_, y);
        y_ = y;
    }

    //операторы bool(eq)
    bool operator==(const Point2d& other) const {
        return x_ == other.x_ && y_ == other.y_;
    }

    bool operator!=(const Point2d& other) const {
        return !(*this == other);
    }

    // cout(str)
    friend ostream& operator<<(ostream& os, const Point2d& point) {
        os << "Point2d(" << point.x_ << ", " << point.y_ << ")";
        return os;
    }

    //присваивание по приколу
    Point2d& operator=(const pair<int, int>& pair) {
        x_ = pair.first;
        y_ = pair.second;
        return *this;
    }
    Point2d& operator=(const Point2d& point) {
        x_ = point.x_;
        y_ = point.y_;
        return *this;
    }

};

class Vector2d {
private:
    int x_;
    int y_;

public:
    //Конструкторы по умолчанию
    Vector2d(int x, int y) : x_(x), y_(y) {}

    Vector2d(const Point2d& start, const Point2d& end)
        : x_(end.x() - start.x()), y_(end.y() - start.y()) {
    }

    // гет
    int x() const { return x_; }
    int y() const { return y_; }

    //матричные операторы - вывод/не изменяется
    int operator[](size_t index) const {
        if (index == 0) return x_;
        if (index == 1) return y_;
        throw out_of_range("Индекс за пределами диапазона");
    }

    //матричные операторы - запись/изменяется
    int& operator[](size_t index) {
        if (index == 0) return x_;
        if (index == 1) return y_;
        throw out_of_range("Индекс за пределами диапазона");
    }

    //Итераторы
    int* begin() { return &x_; }
    int* end() { return &y_ + 1; }

    const int* begin() const { return &x_; }
    const int* end() const { return &y_ + 1; }
    size_t size() const { return 2; }

    //операторы bool
    bool operator==(const Vector2d& other) const {
        return x_ == other.x_ && y_ == other.y_;
    }

    bool operator!=(const Vector2d& other) const {
        return !(*this == other);
    }

    //cout
    friend ostream& operator<<(ostream& os, const Vector2d& vec) {
        os << "Vector2d(" << vec.x_ << ", " << vec.y_ << ")";
        return os;
    }

    //гепотинуза вместо x^2+y^2
    double magnitude() const {
        return hypot(x_, y_);
    }

    //Операторы +/- */ /
    Vector2d operator+(const Vector2d& other) const {
        return Vector2d(x_ + other.x_, y_ + other.y_);
    }

    Vector2d operator-(const Vector2d& other) const {
        return Vector2d(x_ - other.x_, y_ - other.y_);
    }

    Vector2d operator*(int scalar) const {
        return Vector2d(x_ * scalar, y_ * scalar);
    }

    Vector2d operator/(int scalar) const {
        if (scalar == 0) throw invalid_argument("Division by zero");
        return Vector2d(x_ / scalar, y_ / scalar);
    }

    //Скалярное произведение
    int dot(const Vector2d& other) const {
        return x_ * other.x_ + y_ * other.y_;
    }

    //Векторное произведение
    int cross(const Vector2d& other) const {
        return x_ * other.y_ - y_ * other.x_;
    }

    //Скалярное произведение 2x векторов
    static int Dot(const Vector2d& a, const Vector2d& b) {
        return a.dot(b);
    }

    //Векторное произведение 2х векторов
    static int Cross(const Vector2d& a, const Vector2d& b) {
        return a.cross(b);
    }

    //Смешанное произведение 3х векторов
    static int TripleProduct(const Vector2d& a, const Vector2d& b, const Vector2d& c) {
        return a.cross(b) * c.x() + a.cross(b) * c.y();
    }
};

int main() {
    setlocale(LC_ALL,"Rus");
    try {
        //point2d
        cout << "Демонстрация Point2d" << endl;

        Point2d p1(10, 20);
        Point2d p2(30, 40);
        {
            //Point2d p3(-30, 40);

            cout << "Точка p1: " << p1 << endl;
            cout << "Точка p2: " << p2 << endl;

            //сет
            p1.set_x(15);
            cout << "После изменения p1.x: " << p1 << endl;
            p2.set_y(10);
            cout << "После изменения p2.y: " << p2 << endl;


            //сравнения точек
            cout << "p1 == p2 ? " << (p1 == p2) << endl;
            cout << "p1 != p2 ? " << (p1 != p2) << endl;
        }

        //vector2d
        cout << "\nДемонстрация Vector2d" << endl;
        {
            Vector2d v1(1, 2);
            Vector2d v2(3, 4);
            Vector2d v3(p1, p2); // Вектор из p1 в p2

            cout << "Вектор v1: " << v1 << endl;
            cout << "Вектор v2: " << v2 << endl;
            cout << "Вектор из p1 в p2: " << v3 << endl;

            cout << "v1[0] = " << v1[0] << ", v1[1] = " << v1[1] << endl;
            v1[0] = 5;
            cout << "После изменения v1[0]: " << v1 << endl;

            cout << "Компоненты v2: ";
            for (int component : v2) {
                cout << component << " ";
            }
            cout << endl;

            cout << "\n Операции" << endl;
            cout << "v1 + v2 = " << (v1 + v2) << endl;
            cout << "v1 - v2 = " << (v1 - v2) << endl;
            cout << "v1 * 3 = " << (v1 * 3) << endl;
            cout << "v2 / 2 = " << (v2 / 2) << endl;
            cout << "|v1| = " << v1.magnitude() << endl;

            //скалярного и векторного произведения
            cout << "\n Произведения векторов" << endl;
            cout << "v1.dot(v2) = " << v1.dot(v2) << endl;
            cout << "Vector2d::Dot(v1, v2) = " << Vector2d::Dot(v1, v2) << endl;

            cout << "v1.cross(v2) = " << v1.cross(v2) << endl;
            cout << "Vector2d::Cross(v1, v2) = " << Vector2d::Cross(v1, v2) << endl;

            //смешанное
            Vector2d v4(2, 3);
            cout << "Vector2d::TripleProduct(v1, v2, v4) = " << Vector2d::TripleProduct(v1, v2, v4) << endl;
        }


    }
    catch (const exception& e)
    {
        cerr << "Произошла ошибка: " << e.what() << endl;
    }
}