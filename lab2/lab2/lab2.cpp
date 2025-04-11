#include <iostream>
#include <fstream>
#include <string>
#include <map>
#include <vector>
#include <tuple>
#include <locale>

using namespace std;

//30-37: Это коды для изменения цвета текста.
//40-47: Это коды для изменения цвета фона
enum class Color {
    BLACK = 30, 
    RED = 31, 
    GREEN = 32, 
    YELLOW = 33,
    BLUE = 34, 
    MAGENTA = 35, 
    CYAN = 36, 
    WHITE = 37, 
    DEFAULT = 39
};

class ConsolePrinter {
private:
    static map<char, vector<string>> fontData;
    Color color;
    char symbol;

    //позиции для вывода символа
    tuple<int, int> position; //текущая
    //tuple<int, int> originalPos; //позиция куда выводить

    //установить цвет
    void setColor() const {
        cout << "\033[" << static_cast<int>(color) << "m";
    }

    //Сбросить стиль
    static void resetStyle() {
        cout << "\033[0m";
    }

    static void parseFontLine(const string& line, char& currentChar, vector<string>& currentData) {
        if (line.empty()) return;

        if (line[0] == '[' && line.find(']') != string::npos) {
            //Новый сивмол
            currentChar = line[1];
            currentData.clear();
        }
        else if (!line.empty() && currentChar != 0) {
            //Строка данных шрифта
            currentData.push_back(line);
        }
    }

public:
    //загрузка стилей + сделать проверку на русский и английский язык
    static void loadFont(const string& filePath) {
        ifstream file(filePath);
        if (!file.is_open()) {
            throw runtime_error("Ошибка: не удалось открыть файл " + filePath);
        }

        //file.imbue(locale("ru_RU.UTF-8"));

        char currentChar = 0;
        vector<string> currentData;
        string line;

        while (getline(file, line)) {
            //Убрать символ перевода строки
            if (!line.empty() && line.back() == '\r') {
                line.pop_back();
            }

            parseFontLine(line, currentChar, currentData);

            //Если найден неизвестный символ или конец файла - завершаем
            if ((!currentData.empty() && line.empty()) || file.eof()) {
                if (currentChar != 0 && !currentData.empty()) {
                    fontData[currentChar] = currentData;
                    currentData.clear();
                }
            }
        }
    }

    //конструктор
    ConsolePrinter(Color color = Color::DEFAULT, tuple<int, int> pos = { 1, 1 }, char sym = '#')
        : color(color), position(pos), symbol(sym) {
    }
    //деструктор
    ~ConsolePrinter() {
        resetStyle();
    }

    //это статический вывод
    static void print(const string& text, Color color = Color::DEFAULT, tuple<int, int> pos = { 0, 0 }, char sym = '#') {
        ConsolePrinter printer(color, pos, sym);
        printer.printText(text);
    }

    //это для обычного вывода
    void printText(const string& text) {
        setColor();

        //Высота символов
        const int charHeight = fontData.empty() ? 0 : fontData.begin()->second.size();

        // Выводим построчно все символы
        for (int lineNum = 0; lineNum < charHeight; ++lineNum) {
            for (char c : text) {
                c = toupper(c);

                if (c == ' ') {
                    cout << "     "; // 5 пробелов для пробела
                    continue;
                }

                auto it = fontData.find(c);
                if (it != fontData.end() && lineNum < it->second.size()) {
                    for (char ch : it->second[lineNum]) {
                        cout << (ch == '*' ? symbol : ' ');
                    }
                }
                cout << " "; // пробел между символами
            }
            cout << endl; // новая строка после вывода всех символов
        }

        resetStyle();
    }
};

map<char, vector<string>> ConsolePrinter::fontData;

int main() {
    setlocale(LC_ALL, "Russian");

    try {
        //загрузка шрифтов
        ConsolePrinter::loadFont("C://Users//Anneta//source//repos//lab2//text_style.txt");
        ConsolePrinter::loadFont("C://Users//Anneta//source//repos//lab2//text_style_rus.txt");

        ConsolePrinter printer(Color::GREEN, { 1, 1 }, '#');
        printer.printText("ABC АБВ");

        ConsolePrinter printer2(Color::WHITE, { 1, 1 }, '%');
        printer2.printText("ААААААААААААААААА");
    }
    catch (const exception& e) {
        cerr << "Error: " << e.what() << endl;
        return 1;
    }

    return 0;
}