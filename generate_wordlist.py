#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# ты можешь использовать любое опорное слово для создание словоря для своего случая))
# используй с умом сынок
"""
Генератор словаря с базой alona
Сортировка: года, числа, даты, комплексные
"""

import os
from pathlib import Path
import string

def generate_wordlist():
    """Генерирует пароли, отсортированные по популярности"""
    
    passwords = []
    
    # Варианты алона с разными заглавными
    base_variants = []
    base = "alona"
    
    # Основные варианты
    main_variants = ["alona", "Alona", "ALONA"]
    
    # Все комбинации заглавных
    for i in range(32):
        variant = ""
        for j, char in enumerate(base):
            if (i >> j) & 1:
                variant += char.upper()
            else:
                variant += char.lower()
        if variant not in main_variants:
            main_variants.append(variant)
    
    base_variants = main_variants
    
    print(f"Вариантов: {len(base_variants)}")
    
    # Приоритет 1: Года
    for base in base_variants[:3]:
        for year in range(1980, 2010):
            passwords.append(f"{base}{year}")
    
    for base in base_variants[:3]:
        for year in range(1950, 1980):
            passwords.append(f"{base}{year}")
    
    for base in base_variants[:3]:
        for year in range(2010, 2015):
            passwords.append(f"{base}{year}")
    
    print(f"Приоритет 1: {len(passwords)}")
    
    # Приоритет 2: Числа
    simple_suffixes = [
        "123", "1234", "12345",
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "00", "11", "22", "33", "44", "55", "66", "77", "88", "99",
        "007", "666", "777", "888", "999",
        "000", "111", "222", "333",
    ]
    
    for base in base_variants[:5]:
        for suffix in simple_suffixes:
            passwords.append(f"{base}{suffix}")
    
    print(f"Приоритет 2: {len(passwords)}")
    
    # Приоритет 3: Даты DDMM
    for base in base_variants[:3]:
        for age in range(20, 71):
            passwords.append(f"{base}{age:02d}")
    
    popular_dates = [
        (1, 1), (1, 6), (1, 12),
        (8, 3), (23, 2), (12, 4), (9, 5),
        (25, 12), (31, 12), (14, 2),
    ]
    
    for base in base_variants[:3]:
        for day, month in popular_dates:
            passwords.append(f"{base}{day:02d}{month:02d}")
    
    print(f"Приоритет 3: {len(passwords)}")
    
    # Приоритет 4: Все даты
    for base in base_variants[:3]:
        for day in range(1, 32):
            for month in range(1, 13):
                passwords.append(f"{base}{day:02d}{month:02d}")
    
    print(f"Приоритет 4: {len(passwords)}")
    
    # Приоритет 5: 2 цифры года
    for base in base_variants[:3]:
        for year_short in range(50, 100):
            passwords.append(f"{base}{year_short:02d}")
        for year_short in range(0, 25):
            passwords.append(f"{base}{year_short:02d}")
    
    print(f"Приоритет 5: {len(passwords)}")
    
    # Приоритет 6: Полная дата
    for base in base_variants[:3]:
        for day in range(1, 32):
            for month in range(1, 13):
                for year in range(0, 100, 5):
                    passwords.append(f"{base}{day:02d}{month:02d}{year:02d}")
    
    print(f"Приоритет 6: {len(passwords)}")
    
    # Приоритет 7: Год в начале
    for year in range(1970, 2015, 1):
        for base in base_variants[:3]:
            passwords.append(f"{year}{base}")
    
    print(f"Приоритет 7: {len(passwords)}")
    
    # Приоритет 8: 3 цифры
    for base in base_variants:
        for num in range(1000):
            passwords.append(f"{base}{num:03d}")
    
    print(f"Приоритет 8: {len(passwords)}")
    
    # Приоритет 9: Буква + 2 цифры
    for base in base_variants:
        for letter in string.ascii_lowercase:
            for num in range(100):
                passwords.append(f"{base}{letter}{num:02d}")
    
    print(f"Приоритет 9: {len(passwords)}")
    
    # Фильтр на длину 8+
    long_passwords = [pwd for pwd in passwords if len(pwd) >= 8]
    print(f"Отфильтрованно (8+): {len(long_passwords)}")
    
    # Дубликаты
    seen = set()
    unique_passwords = []
    for pwd in long_passwords:
        if pwd not in seen:
            seen.add(pwd)
            unique_passwords.append(pwd)
    
    return unique_passwords

def main():
    # Папка
    dict_dir = Path(r"C:\Users\Cocos\Desktop\2\dictionaries")
    
    # Создаём папку
    dict_dir.mkdir(parents=True, exist_ok=True)
    
    # Файл
    output_file = dict_dir / "alona.txt"
    
    print("Генерация словаря паролей...")
    print("=" * 60)
    
    # Генерируем пароли
    passwords = generate_wordlist()
    
    print("=" * 60)
    
    # Записываем
    print(f"Запись {len(passwords)} паролей...")
    with open(output_file, 'w', encoding='utf-8') as f:
        for password in passwords:
            f.write(password + '\n')
    
    print("=" * 60)
    print(f"ГОТОВО")
    print(f"Файл: {output_file}")
    print(f"Паролей: {len(passwords)}")
    print(f"Размер: {output_file.stat().st_size / 1024:.2f} KB")
    print()
    print("Первые 20:")
    for i in range(min(20, len(passwords))):
        print(f"  {i+1:3d}. {passwords[i]}")

if __name__ == "__main__":
    main()
