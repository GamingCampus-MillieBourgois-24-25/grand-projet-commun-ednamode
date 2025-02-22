# Drip or Drop

## 🎮 About the Game  
**Drip or Drop** is a **multiplayer party game** that blends fashion, creativity, and social deduction. Players compete in various **fast-paced mini-games** where they create, modify, or sabotage outfits while engaging in strategic deception. Whether designing the perfect look or exposing a hidden saboteur, every round offers unique and unpredictable gameplay.

## 🛠️ Development Information  
- **Engine:** Unity 6  
- **Multiplayer Framework:** Netcode for GameObjects  
- **Target Platforms:** iOS & Android  
- **Programming Language:** C#  
- **Data Management:** JSON & Firebase  

## 🎭 Game Modes  
Drip or Drop features **multiple engaging game modes**, each offering a distinct multiplayer experience:  

- **Dress to Impress:** Players design outfits based on a theme and vote for the best one.  
- **Passage de Mode:** Each player starts with an outfit and passes it along, adding their own touch before voting on the final look.  
- **Sabotage Stylé:** One player is secretly chosen as the saboteur, tasked with subtly ruining others' outfits without being detected.  
- **Imposteur du Style:** Players receive a fashion theme—except for the impostor, who must blend in without knowing the theme!  

## 🌍 Multiplayer & Matchmaking  
- **4 to 8 player online matches** with real-time outfit customization and voting.  
- **Matchmaking based on skill level (ELO/Glicko-2)** and connection stability.  
- **Private lobbies and quick-match system.**  
- **AFK handling:** Automatic replacement by bots or waiting for a new player.  

## 🎨 Art & Design  
- **Colorful and expressive 3D character models.**  
- **Dynamic UI with responsive design for mobile devices.**  
- **Stylish and energetic soundtrack with immersive sound effects.**  

## 🔧 Technical Features  
- **Optimized for mobile performance:**  
  - Object pooling for reduced CPU load.  
  - GPU instancing & dynamic batching for smooth rendering.  
  - Low-power mode to extend battery life.  
- **Network Efficiency:**  
  - Compressed Netcode packets for minimal bandwidth usage.  
  - Lag compensation via interpolation/extrapolation for a seamless experience.  
  - Adjustable update frequencies for different events (e.g., animations at high frequency, chat at low frequency).  

## 🛍️ Monetization & Progression  
- **In-app purchases:** Cosmetics, accessories, and premium fashion packs.  
- **Battle Pass system:** Unlockable seasonal content.  
- **Leaderboards and challenges:** Weekly competitions to earn exclusive rewards.  

## 🚀 Future Plans  
- **Controller support (Xbox, PlayStation, Bluetooth).**  
- **Augmented Reality (AR) fashion customization mode.**  
- **Potential WebGL/PC port with adapted controls.**  

## 🐞 Bug Reporting & Support  
- **Built-in bug reporting system** for instant feedback.  
- **Automatic crash logs sent to Firebase for issue tracking.**  
- **Frequent updates & patches without requiring full reinstallation.**  

---

## 📌 How to Run the Project  
### Prerequisites  
- **Unity 6** installed  
- **Netcode for GameObjects** package  
- **Firebase SDK** (for cloud storage and authentication)  

### Installation  
1. Clone the repository:  
   ```bash
   git clone https://github.com/YourRepo/DripOrDrop.git
   cd DripOrDrop
   ```
2. Open the project in **Unity 6**.  
3. Configure Firebase settings in `Assets/Resources/FirebaseConfig.json`.  
4. Run the game in the Unity Editor or build it for iOS/Android.  

---
