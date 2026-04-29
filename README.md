# 🎰 Slot Machine Game (Unity)

## 🎮 Game Overview
This is a simple slot machine game built using Unity.

The player pulls a lever to spin three reels. Each reel displays symbols, and the player wins when all three symbols on the payline match.

The game includes:
- Smooth spinning reel animations
- Randomized outcomes
- Betting system (20 / 40 / 80)
- Currency system with rewards
- Particle effects on win

---

## 🕹️ Gameplay Mechanics

- Player starts with **100 coins**
- Player selects a bet:
  - 20
  - 40
  - 80
- Each spin deducts the selected bet
- If all 3 symbols match:
  - Player wins **2× the bet amount**

---

## Thought Process 

- System Design
  The game splits into clear Systems:
  . SlotMachineController -> handles spin logic
  . CurrencyManager -> handles coins & UI
  . SlotMachineData -> stores symbol data
  . BetButton ->  handles player betting input

- Major Problem
  Problem:
  The 2nd and 3rd reels were always showing the same result.

Cause:
The slot data was being shallow-copied
All reels shared the same underlying data
Movement logic was identical → same outcomes

Solution:
Implemented deep copy of slot data
Added random offsets per spin
Introduced random step variation during spinning

This ensured each reel behaves independently.

---
