#pragma once

#include<string>
#include<variant>
#include<map>
#include<vector>
#include<type_traits>

using namespace std;

class JObject;

using JType = variant<int, double, bool, string, JObject>;

class JObject
{
private:
	map<string, JType> values;
	
	const JType getValue(const string&) const;
public:
	JObject()
	{
		values = {};
	}

	JObject(const string& json) : JObject()
	{
		read(json);
	}

	JObject(const JObject& other) : values(other.values) {}
	JObject(JObject&& other) noexcept : values(move(other.values)) {}
	
	JObject& operator=(const JObject& other)
	{
		values = other.values;

		return *this;
	}

	JObject& operator=(JObject&& other) noexcept
	{
		values = move(other.values);

		return *this;
	}

	void read(const string&);

	operator string() const;

	template<typename T, typename = enable_if<is_same_v<T, int> || is_same_v<T, double> || is_same_v<T, bool> || is_same_v<T, string> || is_same_v<T, JObject>>>
	inline T& get(string&& name)
	{
		return get<T>(values[name]);
	}

	template<typename T, typename = enable_if<is_same_v<T, int> || is_same_v<T, double> || is_same_v<T, bool> || is_same_v<T, string> || is_same_v<T, JObject>>>
	inline void add(string&& name, T value)
	{
		values.insert_or_assign(name, JType(value));
	}

	inline JType& operator[] (string&& name)
	{
		return values[name];
	}

	inline bool remove(string&& name)
	{
		return values.erase(name) >= -1;
	}

	~JObject() = default;
};

