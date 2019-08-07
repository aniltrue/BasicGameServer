#pragma once
#include <string>
#include <vector>
#include <chrono>
#include <thread>
#include <mutex>
#include "JObject.h"

using namespace std;

class Player
{
private:
	int id;
	string name;
	double x, y, dirX, dirY, targetX, targetY, range, attackAngle, attackSpeed, moveSpeed, attackDamage, health;
	bool attacking, isAlive;
	double attackMS;

	mutex playerMutex;

	JObject message;

	string sendMessage;

	void send(const string) const;
	JObject& receive() const;
public:
	Player();

	Player(const Player&) = delete;
	Player& operator=(const Player&) = delete;

	Player(Player&& other);
	Player& operator=(Player&& other);

	void update(const string& json);

	void run();

	void move(const int& now);
	void calculateAttackMS(const int& now);
	void attack(vector<Player>& players);
	void acceptAttack(Player& player, double attack);

	inline operator string() const
	{
		auto guard = lock_guard<mutex>(playerMutex);

		return (string)message;
	}

	inline void updateMessage(const string newMessage)
	{
		playerMutex.lock();

		sendMessage = newMessage;

		playerMutex.unlock();
	}

	~Player() = default;
};

