#include <string>
#include <sstream>

// Workaround for Android NDK builds (version r10e) that does not support std::to_string and std::strold so far
namespace std
{
	template <typename T>
	std::string to_string(T Value)
	{
		std::ostringstream TempStream;
		TempStream << Value;
		return TempStream.str();
	}		
}