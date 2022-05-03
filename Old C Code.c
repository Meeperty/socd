#include <windows.h>
#include <stdio.h>
#include <ctype.h>
#include <shlwapi.h>
#include <stdbool.h>
#pragma comment(lib,"user32.lib")
#pragma comment(lib,"shlwapi.lib")

// Maintaining our own key states bookkeeping is kinda cringe
// but we can't really use Get[Async]KeyState, see the first note at
// https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
# define KEY_LEFT 0
# define KEY_RIGHT 1
# define KEY_UP 2
# define KEY_DOWN 3
# define IS_DOWN 1
# define IS_UP 0
# define whitelist_max_length 200

const char* CONFIG_NAME = "socd.conf";
const LPCWSTR CLASS_NAME = L"SOCD_CLASS";
char config_line[100];
char focused_program[MAX_PATH];
char programs_whitelist[whitelist_max_length][MAX_PATH] = { 0 };

HHOOK kbhook;
int hook_is_installed = 0;

int real[4]; // whether the key is pressed for real on keyboard
int virtualKeys[4]; // whether the key is pressed on a software level
//              a     d     w     s
int WASD[4] = { 0x41, 0x44, 0x57, 0x53 };
const int WASD_ID = 100;
//                <     >     ^     v
int ARROWS[4] = { 0x25, 0x27, 0x26, 0x28 };
const int ARROWS_ID = 200;

int CUSTOM_BINDS[4];
const int CUSTOM_ID = 300;

int error_message(char* text) {
    int error = GetLastError();
    // error is dword which is 32 bits, so 10 characters should be enough considering that we have an extra %d in the string
    char* error_buffer = malloc(strlen(text) + 10);
    sprintf(error_buffer, text, error);
    MessageBox(
        NULL,
        error_buffer,
        "RIP",
        MB_OK | MB_ICONERROR);
    return 1;
}

void write_settings(int* bindings) {
    FILE* config_file = fopen(CONFIG_NAME, "w");
    if (config_file == NULL) {
        // This writes to console that we're freeing sigh
        // Probably better to show MessageBox
        perror("Couldn't open the config file");
        return;
    }
    for (int i = 0; i < 4; i++) {
        fprintf(config_file, "%X\n", bindings[i]);
    }
    for (int i = 0; i < whitelist_max_length; i++) {
        if (programs_whitelist[i][0] == '\0') {
            break;
        }
        fprintf(config_file, "%s\n", programs_whitelist[i]);
    }
    fclose(config_file);
}

void set_bindings(int* bindings) {
    CUSTOM_BINDS[0] = bindings[0];
    CUSTOM_BINDS[1] = bindings[1];
    CUSTOM_BINDS[2] = bindings[2];
    CUSTOM_BINDS[3] = bindings[3];
}

void read_settings() {
    FILE* config_file = fopen(CONFIG_NAME, "r+");
    if (config_file == NULL) {
        set_bindings(WASD);
        write_settings(WASD);
        return;
    }

    // First 4 lines are key bindings
    for (int i = 0; i < 4; i++) {
        char* result = fgets(config_line, 100, config_file);
        int button = (int)strtol(result, NULL, 16);
        CUSTOM_BINDS[i] = button;
    }

    // Then there are programs SOCD cleaner should track
    int i = 0;
    while (fgets(programs_whitelist[i], MAX_PATH, config_file) != NULL) {
        // Remove line ends from the line we just read.
        // Works for LF, CR, CRLF, LFCR etc
        programs_whitelist[i][strcspn(programs_whitelist[i], "\r\n")] = 0;
        i++;
    }
    for (int i = 0; i < whitelist_max_length; i++) {
        if (programs_whitelist[i][0] == '\0') {
            break;
        }
    }
    fclose(config_file);
}

int find_opposing_key(int key) {
    if (key == CUSTOM_BINDS[KEY_LEFT]) {
        return CUSTOM_BINDS[KEY_RIGHT];
    }
    if (key == CUSTOM_BINDS[KEY_RIGHT]) {
        return CUSTOM_BINDS[KEY_LEFT];
    }
    if (key == CUSTOM_BINDS[KEY_UP]) {
        return CUSTOM_BINDS[KEY_DOWN];
    }
    if (key == CUSTOM_BINDS[KEY_DOWN]) {
        return CUSTOM_BINDS[KEY_UP];
    }
    return -1;
}

int find_index_by_key(int key) {
    if (key == CUSTOM_BINDS[KEY_LEFT]) {
        return KEY_LEFT;
    }
    if (key == CUSTOM_BINDS[KEY_RIGHT]) {
        return KEY_RIGHT;
    }
    if (key == CUSTOM_BINDS[KEY_UP]) {
        return KEY_UP;
    }
    if (key == CUSTOM_BINDS[KEY_DOWN]) {
        return KEY_DOWN;
    }
    return -1;
}

LRESULT CALLBACK LowLevelKeyboardProc(int nCode, WPARAM wParam, LPARAM lParam) {
    KBDLLHOOKSTRUCT* kbInput = (KBDLLHOOKSTRUCT*)lParam;

    // We ignore injected events so we don't mess with the inputs
    // we inject ourselves with SendInput
    if (nCode != HC_ACTION || kbInput->flags & LLKHF_INJECTED) {
        return CallNextHookEx(NULL, nCode, wParam, lParam);
    }

    INPUT input;
    int key = kbInput->vkCode;
    int opposing = find_opposing_key(key);
    if (opposing < 0) {
        return CallNextHookEx(NULL, nCode, wParam, lParam);
    }
    int index = find_index_by_key(key);
    int opposing_index = find_index_by_key(opposing);

    // Holding Alt sends WM_SYSKEYDOWN/WM_SYSKEYUP
    // instead of WM_KEYDOWN/WM_KEYUP, check it as well
    if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) {
        real[index] = IS_DOWN;
        virtualKeys[index] = IS_DOWN;
        if (real[opposing_index] == IS_DOWN && virtualKeys[opposing_index] == IS_DOWN) {
            input.type = INPUT_KEYBOARD;
            input.ki = (KEYBDINPUT){ opposing, 0, KEYEVENTF_KEYUP, 0, 0 };
            SendInput(1, &input, sizeof(INPUT));
            virtualKeys[opposing_index] = IS_UP;
        }
    }
    else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP) {
        real[index] = IS_UP;
        virtualKeys[index] = IS_UP;
        if (real[opposing_index] == IS_DOWN) {
            input.type = INPUT_KEYBOARD;
            input.ki = (KEYBDINPUT){ opposing, 0, 0, 0, 0 };
            SendInput(1, &input, sizeof(INPUT));
            virtualKeys[opposing_index] = IS_DOWN;
        }
    }
    return CallNextHookEx(NULL, nCode, wParam, lParam);
}

void set_kb_hook(HINSTANCE instance) {
    if (!hook_is_installed) {
        printf("hooking this shit\n");
        kbhook = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, instance, 0);
        if (kbhook != NULL) {
            hook_is_installed = 1;
        }
        else {
            printf("hook failed oopsie\n");
        }
    }
}

void unset_kb_hook() {
    if (hook_is_installed) {
        printf("unhooking this shit\n");
        UnhookWindowsHookEx(kbhook);
        // Forget buttons that are pressed before unhooking from the keyboard
        // So that when we hook again we don't end up in a dirty state where one
        // of the buttons is being stuck from the previous hook
        real[KEY_LEFT] = IS_UP;
        real[KEY_RIGHT] = IS_UP;
        real[KEY_UP] = IS_UP;
        real[KEY_DOWN] = IS_UP;
        virtualKeys[KEY_LEFT] = IS_UP;
        virtualKeys[KEY_RIGHT] = IS_UP;
        virtualKeys[KEY_UP] = IS_UP;
        virtualKeys[KEY_DOWN] = IS_UP;
        hook_is_installed = 0;
    }
}

void get_focused_program() {
    HWND inspected_window = GetForegroundWindow();
    DWORD process_id = 0;
    GetWindowThreadProcessId(inspected_window, &process_id);
    if (process_id == 0) {
        // Sometimes when you minimize a window nothing is focused for a brief moment,
        // in this case windows sends "System Idle Process" as currently focused window
        // for some reason. Just ignore it
        return;
    }
    HANDLE hproc = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_QUERY_LIMITED_INFORMATION, 0, process_id);
    if (hproc == NULL) {
        // Sometimes we can't open a process (#5 "access denied"). Ignore it I guess.
        if (GetLastError() == 5) {
            return;
        }
        error_message("Couldn't open active process, error code is: %d");
    }
    DWORD filename_size = MAX_PATH;
    // This function API is so fucking weird. Read its docs extremely carefully
    QueryFullProcessImageName(hproc, 0, focused_program, &filename_size);
    CloseHandle(hproc);
    PathStripPath(focused_program);
    printf("Window activated: %s\n", focused_program);
}

void detect_focused_program(
    HWINEVENTHOOK hWinEventHook,
    DWORD event,
    HWND window,
    LONG idObject,
    LONG idChild,
    DWORD idEventThread,
    DWORD dwmsEventTime
) {
    // We're ignoring window here, sometimes it points to the program that
    // is not actually being focused for some reason. Instead, we treat the event
    // as a trigger to check for the current foreground program ourselves.
    get_focused_program();

    HINSTANCE hInstance = (HINSTANCE)GetModuleHandle(NULL);
    // Linear scan let's fucking go, don't look here CS degree people
    for (int i = 0; i < whitelist_max_length; i++) {
        if (strcmp(focused_program, programs_whitelist[i]) == 0) {
            set_kb_hook(hInstance);
            return;
        }
        else if (programs_whitelist[i][0] == '\0') {
            break;
        }
    }
    unset_kb_hook();
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    switch (uMsg) {
    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;
    case WM_COMMAND:
        if (wParam == WASD_ID) {
            set_bindings(WASD);
            write_settings(WASD);
        }
        else if (wParam == ARROWS_ID) {
            set_bindings(ARROWS);
            write_settings(ARROWS);
        }
    }

    return DefWindowProcW(hwnd, uMsg, wParam, lParam);
}