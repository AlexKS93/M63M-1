# M63M-1




## Установка

1. Скопируйте скомпилированную программу в любой каталог;
2. Установите сервис с помощью InstallUtil;
3. Укажите настройки;
4. Запустите сервис

## Настройки

Настройки могут быть указаны в файле "AnemorumbometerService.exe.config".

## Схема
```mermaid
graph LR
A[M63M-1] -- RS-232 --> B(Driver)-- OPC --> C(OPC Server)

```
