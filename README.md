

## 🧠 Thought Process 

### System Design
The game is structured into modular systems for clarity and scalability:

- **SlotMachineController** → Handles spin logic and reel behavior  
- **CurrencyManager** → Manages player coins and UI updates  
- **SlotMachineData** → Stores and provides symbol data  
- **BetButton** → Handles player betting input  

This separation ensures clean architecture and easier debugging.

---

### Major Problem Faced

**Problem:**  
The 2nd and 3rd reels were always producing identical results.

**Cause:**  
- Slot data was being shallow-copied  
- Multiple reels shared the same underlying data  
- Identical movement logic resulted in identical outcomes  

**Solution:**  
- Implemented a **deep copy** of slot data for each reel  
- Added **random offsets** at the start of each spin  
- Introduced **random step variation** during spinning  

This ensured each reel behaves independently and produces proper randomized outcomes.

---

### ✨ Visual Enhancements (Post-Processing)

To improve the visual quality and overall feel of the game, post-processing effects were added:

- **Bloom** → Enhances brightness and highlights winning visuals  
- **Vignette** → Focuses attention toward the center of the screen  
- **Chromatic Aberration** → Adds subtle visual distortion for a stylized effect  
- **Distortion** → Gives a dynamic feel during gameplay and spin moments  

These effects help create a more polished and engaging player experience.
