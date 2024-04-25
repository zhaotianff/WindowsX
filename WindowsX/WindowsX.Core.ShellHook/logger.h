#pragma once
#include"framework.h"
#include<fstream>

namespace shellhook
{
	class logger
	{
	public:
		logger();
		~logger();
		bool SetLogFilePath(std::string path);
		bool WriteLog(std::string log);
		void SetEnable(bool is_enable);

	private:
		bool is_enable;
		std::ofstream ofs;
		std::string path;
	};
}
