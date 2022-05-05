#pragma once

#ifdef WindowsSOCD_EXPORTS
#define WindowsSOCD_API __declspec(dllexport)
#else
#define WindowsSOCD_API __declspec(dllimport)
#endif

extern "C" 