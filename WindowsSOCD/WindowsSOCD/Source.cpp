#pragma region Definitions & Inclusions

#include "pch.h"
#include <windows.h>
#include <stdio.h>
#include <ctype.h>
#include <shlwapi.h>
#include <stdbool.h>
#pragma comment(lib,"user32.lib")
#pragma comment(lib,"shlwapi.lib")

# define KEY_LEFT 0
# define KEY_RIGHT 1
# define KEY_UP 2
# define KEY_DOWN 3
# define IS_DOWN 1
# define IS_UP 0
# define whitelist_max_length 200

#pragma endregion

const char* CONFIG_NAME = "socd.conf";