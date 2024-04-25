#include "pch.h"
#include "logger.h"

namespace shellhook
{
    logger::logger()
    {
        is_enable = false;
    }

    logger::~logger()
    {
        if (ofs.is_open())
            ofs.close();
    }

    bool logger::SetLogFilePath(std::string path)
    {
        this->path = path;
        ofs.open(path, std::ios::out | std::ios::app);

        return ofs.is_open();
    }

    bool logger::WriteLog(std::string log)
    {
        if (is_enable)
        {

            ofs << log << std::endl;
            return true;
        }
        return false;
    }

    void logger::SetEnable(bool is_enable)
    {
        this->is_enable = is_enable;
    }

}
