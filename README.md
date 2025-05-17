# 🎮 SW_Capstone-Design

Unity를 통한 2D 로그라이크 장르의 게임입니다.


### 🤝 팀원 소개 PixelForge
이 프로젝트는 스포트웨어캡스톤디자인 **PixelForge** 팀원들이 협력하여 개발했습니다.

<table>
  <tbody>
    <tr>
      <td align="center"><a href="https://github.com/GithubOfHermes">
        <img src="https://github.com/GithubOfHermes.png?size=100" width="100px;" alt=""/><br /><sub><b>김성열</b></sub></a><br />
      </td>
      <td align="center"><a href="https://github.com/inNewPG">
        <img src="https://github.com/inNewPG.png?size=100" width="100px;" alt=""/><br /><sub><b>박하민</b></sub></a><br />
      </td>
      <td align="center"><a href="https://github.com/Ryder76524">
        <img src="https://github.com/Ryder76524.png?size=100" width="100px;" alt=""/><br /><sub><b>김대명</b></sub></a><br />
      </td>
    </tr>
  </tbody>
</table>

## ✨ 주요 기능

🛍️ 상점 물품 및 상자 보상 아이템 소개

🩺 체력 회복
체력 회복 +10  → currentHP + 10 >= maxHP인 경우, currentHP == maxHP로 설정됩니다.
  
🗡️ 기본 스탯 증가
- 공격력 +5
  
- 스킬 데미지 +5
  
- 최대 체력 +10
  
  → maxHP를 10 증가시키며, currentHP도 +10 회복합니다. 단, currentHP + 10 >= maxHP인 경우 currentHP == maxHP로 조정됩니다.
  
- 골드 획득량 +1
  
🧬 에픽 스탯 (희귀 아이템)
- 대시 횟수 +1
  
  → 단 1회만 구매/획득 가능하며, 이후에는 상점에 등장하지 않습니다.
  
- 스킬 쿨타임 10% 감소
  
  → 최대 50%까지 누적 감소 가능. 50% 이상이면 상점에 등장하지 않음.
  
- 피해량 10% 감소
  
  → 최대 50%까지 누적 감소 가능. 50% 이상이면 상점에 등장하지 않음.
  
- 대시 게이지 충전 속도 0.5초 감소
  
  → 최대 1초까지 감소 가능. 최대 2회까지 구매/획득 가능.
