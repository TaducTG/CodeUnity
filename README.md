# 🌱 2D Sandbox Top-down Game (Unity)

Một tựa game 2D sandbox với góc nhìn **top-down** được phát triển bằng Unity.  
Game kết hợp nhiều yếu tố **Stardew Valley** và **Minecraft**: vừa có cơ chế sandbox, vừa có NPC, quest, và các hoạt động như trồng trọt, chế tạo, chiến đấu.  

---

## ✨ Tính năng đã tích hợp

### 🎒 Hệ thống Inventory & Trang bị
- Giao diện **inventory** trực quan với **drag & drop**.
- Hỗ trợ **stack item**, hoán đổi slot, và kiểm tra vị trí hợp lệ khi thả.
- **Chest system**: lưu trữ và lấy đồ qua rương.
- **Equipment slots**: trang bị vũ khí, áo giáp, phụ kiện → ảnh hưởng trực tiếp đến chỉ số nhân vật.

---

### 🔨 Hệ thống Crafting & Farming
- **Crafting system**: chế tạo vật phẩm từ các công thức có sẵn.
- Giao diện công thức (recipe UI) hiển thị nguyên liệu yêu cầu.
- **Farming system**: trồng trọt cây nông sản theo thời gian → thu hoạch để chế biến hoặc bán.

---

### 💰 Hệ thống Kinh tế
- **Shop system**: mua và bán vật phẩm với NPC thương nhân.
- Giao diện cửa hàng trực quan, hỗ trợ kéo thả item giữa inventory và shop.

---

### 🧑‍🤝‍🧑 Hệ thống Quest & NPC
- NPC có **đối thoại** (dialogue system).
- **Quest system**: nhận, theo dõi và hoàn thành nhiệm vụ → phần thưởng.
- Quest gắn liền với NPC, giúp mở rộng cốt truyện và gameplay.

---

### 📦 Hệ thống Item
- Quản lý vật phẩm bằng **ScriptableObject**:
  - Consumable (item tiêu hao: hồi máu, mana…)
  - Equipment (vũ khí, giáp…)
  - Resource (nguyên liệu crafting, farming)
- Dễ dàng mở rộng và thêm loại item mới.

---

### 🗺️ Hệ thống Spawnmap
- **Procedural map generation** bằng **Perlin Noise**.
- Kết hợp **Tilemap** cho nền và **Prefab spawn** (cây, đá, tài nguyên).
- Hệ thống **Chunk-based loading**: chỉ load/unload các vùng map gần player → tăng hiệu suất, giảm lag.

---

### 🐾 Hệ thống Mobs
- **Animals**: động vật hiền, sinh sản và cung cấp tài nguyên.
- **Enemy mobs**: quái vật sinh ngẫu nhiên ở vùng tối/nguy hiểm.
- **Boss mobs**: kẻ thù đặc biệt với cơ chế AI phức tạp, phần thưởng lớn.

---

## 🛠️ Công nghệ
- Unity (2D Top-down)
- C# (OOP + ScriptableObject)
- Tilemap + Prefab Spawner
- Chunk-based optimization

