# ProjectPuzzle


Tải maven Link: maven.apache.org/download.cgi Tìm dòng Binary zip archive. Bấm vào link tải file .zip. Chọn apache-maven-3.9.12-bin.zip.

Tải git(Bắt buộc) https://git-scm.com/download/win -> Chọn click here to download, tải xong mở file đó lên, nhấn next hết cho đến khi có chữ install là được

Tổng quan toàn bộ quy trình dùng git:

Clone dự án về máy(lệnh này chỉ dùng cho lần lấy dự án lần đầu tiên, tức là trong máy ae chưa có dự án, còn khi clone xong rồi thì trong máy ae đã có dự án rồi, những lần sau không dùng clone nữa)

git clone https://github.com/ngocquang339/BookStore.git

Sau khi clone xong, dùng lệnh này để chuyển dự án clone thành 1 branch(nhánh) để code chức năng(Phải chuyến thành nhánh thì giả dụ khi code bị sai hoặc lỗi cũng không ảnh hưởng đến main)

git checkout -b feature/login

(Thay feature/login bằng tên chức năng ae làm, ví dụ: feature/cart, feature/payment...)

(Khi ae clone lần đầu về máy thì dùng lệnh này luôn(sau khi dùng lệnh clone bên trên) vì lúc đấy đương nhiên dự án sẽ là phiên bản mới nhất)

2.1 Nếu sau khi ae clone và chức năng này ae chưa code xong nhưng người khác đã code xong chức năng của họ rồi và up vào main, thì lúc đấy phiên bản dự án của ae lúc clone về đã bị cũ. Nên mỗi lần vào code thì ae pull bản code mới nhất về cho chắc ăn, dùng lần lượt các lệnh bên dưới:

git add .

git commit -m "chuc nang dang nhap"

(2 lệnh này là để ae lưu code của chức năng ae đang code dở, tránh bị mất khi pull phiên bản dự án mới nhất về)

Sau đó dùng 2 lệnh này:

 git checkout main
 
 git pull origin main
Sau đó dùng 2 lệnh này để chuyển sang nhánh chức năng ae đang làm để làm tiếp như bình thường:

 git checkout (tên chức năng)
 
 git merge main(Để lấy code mới nhất từ main đắp vào chức năng ae đang làm)
 
 (Nếu 2 người cùng sửa 1 dòng code thì khi merge sẽ bị conflict, git không biết nên lấy code của ai nên bị conflict)
 
 Cách sửa:
 
   Nhìn vào đoạn code bị đánh dấu.Nếu thấy code của Main ngon hơn thì xóa code của mình đi, giữ code Main. Nếu thấy code của mình ngon hơn xóa code Main đi. Nếu cần cả hai thì sửa lại khéo léo để giữ cả hai logic.
   
   Mẹo: Trong VS Code nó có hiện mấy cái nút nhỏ nhỏ ở trên dòng conflict: "Accept Current Change" (Lấy của mình), "Accept Incoming Change" (Lấy của Main). Sau đó dùng 2 lệnh này:
   
     git add .
     
     git commit -m "Da fix conflict"
     
   Chung quy lại thì nên chia việc rõ ràng để tránh việc 2 người cùng sửa 1 dòng code.
Sau khi ae code xong chức năng của mình, thì dùng các lệnh sau để push code lên git:

git add .

git commit -m "Hoan thanh chuc nang dang nhap"

git push origin (tên chức năng)

Sau khi push xong, code vẫn nằm riêng ở nhánh con. Để gộp nó vào dự án chính (main), ae làm như sau:

Truy cập vào trang GitHub của dự án.

Ae sẽ thấy một thanh thông báo màu vàng/xanh hiện lên: "Compare & pull request". Bấm vào đó.

Viết tiêu đề (VD: Merge chức năng Login vào Main).

Bấm Create pull request.

Review code: chưa up vào main vội, xong chức năng nào thì ae trao đổi xem còn thiéu gì, sau khi thống nhất ok rồi thì mới merge vào main

Nếu ok, bấm nút màu xanh lá Merge pull request -> Confirm merge.
