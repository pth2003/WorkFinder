# WorkFinder - Ứng Dụng Cổng Thông Tin Việc Làm

## 1. Giới Thiệu

WorkFinder là một cổng thông tin việc làm trực tuyến toàn diện kết nối người tìm việc với nhà tuyển dụng. Nền tảng này cho phép các công ty đăng tuyển việc làm và người tìm việc có thể tìm kiếm, lọc và ứng tuyển vào các vị trí phù hợp với kỹ năng và sở thích của họ. Tài liệu này phác thảo các khía cạnh kỹ thuật, kiến trúc, tính năng, trường hợp sử dụng và phương pháp phát triển của ứng dụng WorkFinder.

## 2. Kiến Trúc Hệ Thống

### 2.1 Công Nghệ Sử Dụng

- **Framework Backend**: ASP.NET Core 9.0
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Cơ Sở Dữ Liệu**: PostgreSQL
- **ORM**: Entity Framework Core 9.0
- **Xác Thực**: ASP.NET Core Identity
- **Thành Phần UI**: View Components, Partial Views
- **AJAX**: Fetch API cho các yêu cầu bất đồng bộ

### 2.2 Cấu Trúc Dự Án

Ứng dụng tuân theo mô hình kiến trúc phân lớp:

- **Presentation Layer**: Views, View Components, JavaScript
- **Bussiness Logic Layer**: Controllers, Services
- **Data Access Layer**: Repositories, DbContext
- **Domain Model**: Các lớp Entity

### 2.3 Sơ Đồ Quan Hệ Thực Thể

Ứng dụng được xây dựng xung quanh các thực thể cốt lõi sau:

- **ApplicationUser**: Mở rộng từ IdentityUser để lưu trữ dữ liệu người tìm việc và nhà tuyển dụng
- **Company**: Lưu trữ thông tin về nhà tuyển dụng
- **Job**: Chứa chi tiết công việc và yêu cầu
- **JobApplication**: Ghi lại đơn ứng tuyển của người dùng
- **Resume**: Lưu trữ hồ sơ của người tìm việc
- **SavedJob**: Lưu công việc để xem sau
- **Category**: Danh mục công việc
- **JobCategory**: Quan hệ nhiều-nhiều giữa công việc và danh mục

## 3. Thiết Kế Cơ Sở Dữ Liệu

### 3.1 Các Bảng

1. **users**: Bảng người dùng Identity được mở rộng với thông tin người tìm việc/nhà tuyển dụng
2. **companies**: Thông tin công ty bao gồm tên, mô tả, logo, v.v.
3. **jobs**: Danh sách việc làm với tiêu đề, mô tả, yêu cầu, mức lương, v.v.
4. **job_applications**: Hồ sơ ứng tuyển của người dùng vào các công việc
5. **resumes**: Hồ sơ người dùng với học vấn, kinh nghiệm, kỹ năng, v.v.
6. **saved_jobs**: Công việc được người dùng lưu để xem sau
7. **categories**: Danh mục công việc như IT, Tài chính, Marketing, v.v.
8. **job_categories**: Bảng liên kết cho quan hệ nhiều-nhiều

### 3.2 Mối Quan Hệ

- User-Resume: Một-Một
- User-Company: Một-Một (quan hệ nhà tuyển dụng)
- Company-Job: Một-Nhiều
- Job-JobApplication: Một-Nhiều
- User-JobApplication: Một-Nhiều
- User-SavedJob: Một-Nhiều
- Job-SavedJob: Một-Nhiều
- Job-Category: Nhiều-Nhiều (qua job_categories)

## 4. Tính Năng Chính

### 4.1 Xác Thực Người Dùng

- Đăng ký và đăng nhập người dùng sử dụng ASP.NET Core Identity
- Phân quyền dựa trên vai trò (người tìm việc, nhà tuyển dụng, quản trị viên)
- Chính sách mật khẩu bảo mật

### 4.2 Quản Lý Công Việc

- Đăng, chỉnh sửa và quản lý danh sách công việc
- Tìm kiếm và lọc công việc theo nhiều tiêu chí
- Tìm kiếm thời gian thực với chức năng debounce
- Xem chi tiết công việc và các công việc liên quan

### 4.3 Ứng Tuyển Công Việc

- Ứng tuyển công việc với thư xin việc
- Tải lên và quản lý hồ sơ
- Theo dõi trạng thái ứng tuyển

### 4.4 Hồ Sơ Công Ty

- Tạo và quản lý hồ sơ công ty
- Hiển thị chi tiết công ty và danh sách việc làm đang hoạt động
- Trạng thái xác minh công ty

### 4.5 Tìm Kiếm Và Lọc Nâng Cao

- Tìm kiếm dựa trên từ khóa và vị trí
- Lọc theo loại công việc, cấp độ kinh nghiệm, phạm vi lương
- Lọc theo danh mục
- Các tùy chọn lọc nâng cao

## 5. Trường Hợp Sử Dụng

### 5.1 Trường Hợp Sử Dụng Của Người Tìm Việc

1. **Đăng Ký và Quản Lý Hồ Sơ**

   - Đăng ký tài khoản mới
   - Tạo/cập nhật hồ sơ cá nhân
   - Tải lên và quản lý hồ sơ

2. **Tìm Kiếm Công Việc**

   - Tìm kiếm công việc bằng từ khóa và vị trí
   - Lọc công việc theo danh mục, loại công việc, cấp độ kinh nghiệm, lương
   - Lưu công việc thú vị để xem sau
   - Xem chi tiết công việc

3. **Quy Trình Ứng Tuyển**
   - Ứng tuyển công việc
   - Viết thư xin việc
   - Theo dõi trạng thái ứng tuyển
   - Xem lịch sử ứng tuyển

### 5.2 Trường Hợp Sử Dụng Của Nhà Tuyển Dụng

1. **Quản Lý Công Ty**

   - Tạo/cập nhật hồ sơ công ty
   - Thêm logo và chi tiết công ty

2. **Đăng Tuyển Công Việc**

   - Tạo danh sách việc làm mới
   - Chỉnh sửa/xóa danh sách hiện có
   - Thiết lập yêu cầu và phúc lợi công việc

3. **Quản Lý Ứng Viên**
   - Xem ứng viên cho công việc đã đăng
   - Xem xét hồ sơ và thư xin việc
   - Cập nhật trạng thái ứng tuyển

### 5.3 Trường Hợp Sử Dụng Của Quản Trị Viên

1. **Quản Lý Người Dùng**

   - Quản lý tài khoản người dùng
   - Phân công vai trò

2. **Quản Lý Nội Dung**

   - Xem xét và xác minh công ty
   - Kiểm duyệt danh sách việc làm

3. **Quản Lý Danh Mục**
   - Tạo và quản lý danh mục công việc

## 6. Chi Tiết Triển Khai

### 6.1 Mẫu Repository

Ứng dụng triển khai mẫu repository để trừu tượng hóa logic truy cập dữ liệu:

- **IRepository<T>**: Giao diện repository chung cho các thao tác CRUD
- **Repository<T>**: Triển khai chung cho các thực thể cơ bản
- **Các repository chuyên biệt**: JobRepository, CompanyRepository, v.v. cho các thao tác cụ thể với từng thực thể

Các phương thức repository bao gồm:

- GetById, GetAll, Add, Update, Delete (từ repository chung)
- Các phương thức chuyên biệt như GetJobsByFilterAsync, GetJobsByCompanyAsync, v.v.

### 6.2 Triển Khai Tìm Kiếm Thời Gian Thực

Chức năng tìm kiếm công việc sử dụng phương pháp AJAX với debounce:

1. Người dùng nhập tiêu chí tìm kiếm
2. Hàm debounce JavaScript đợi cho đến khi người dùng ngừng gõ (800ms)
3. Yêu cầu AJAX được gửi đến máy chủ
4. Máy chủ lọc công việc dựa trên tiêu chí
5. Kết quả được trả về dưới dạng partial view
6. DOM được cập nhật với kết quả mới mà không cần tải lại trang

Triển khai này cung cấp trải nghiệm người dùng mượt mà với:

- Giảm yêu cầu đến máy chủ
- Phản hồi ngay lập tức
- Không cần tải lại toàn bộ trang

Các thành phần mã:

- utils.js với hàm tiện ích debounce
- Endpoint controller AJAX (JobList action trong JobController)
- Partial view (\_JobListPartial.cshtml)

### 6.3 View Components

Ứng dụng sử dụng view components cho các phần tử UI có thể tái sử dụng:

- **Search**: Form tìm kiếm công việc
- **AdvancedFilter**: Tùy chọn tìm kiếm nâng cao
- **FeaturedJob**: Hiển thị các công việc nổi bật
- **Category**: Hiển thị danh mục công việc
- **Hero**: Phần hero trên trang chủ
- **TopCompany**: Hiển thị các công ty nổi bật

### 6.4 Xác Thực và Phân Quyền

Xác thực được triển khai sử dụng ASP.NET Core Identity:

- Lớp ApplicationUser tùy chỉnh mở rộng từ IdentityUser
- Xác thực dựa trên cookie
- Cấu hình chính sách mật khẩu
- Các form đăng nhập/đăng ký tùy chỉnh

### 6.5 Phân Trang

Danh sách công việc triển khai phân trang phía máy chủ:

1. Kích thước trang và trang hiện tại được theo dõi trong ViewBag
2. Máy chủ chỉ trả về trang kết quả hiện tại
3. UI cung cấp điều hướng giữa các trang
4. AJAX cập nhật khi chuyển trang mà không cần tải lại toàn bộ

## 7. Quy Trình Làm Việc

### 7.1 Quy Trình Tìm Kiếm Công Việc

1. Người dùng nhập tiêu chí tìm kiếm (từ khóa, vị trí, danh mục)
2. Hàm debounce đợi cho đến khi việc gõ dừng lại
3. Yêu cầu AJAX được gửi đến endpoint JobList
4. Máy chủ lọc công việc dựa trên tiêu chí
5. Kết quả được trả về dưới dạng partial view
6. DOM được cập nhật với kết quả mới

### 7.2 Quy Trình Ứng Tuyển Công Việc

1. Người dùng xem chi tiết công việc
2. Người dùng nhấp vào nút "Ứng tuyển"
3. Người dùng được yêu cầu đăng nhập nếu chưa xác thực
4. Người dùng nhập thư xin việc và chọn hồ sơ
5. Đơn ứng tuyển được gửi đến máy chủ
6. Máy chủ tạo bản ghi JobApplication
7. Người dùng được chuyển hướng đến trang xác nhận

### 7.3 Quy Trình Đăng Tuyển Công Việc (Nhà Tuyển Dụng)

1. Nhà tuyển dụng đăng nhập vào tài khoản
2. Điều hướng đến phần quản lý công việc
3. Tạo danh sách việc làm mới với chi tiết
4. Công việc được lưu vào cơ sở dữ liệu
5. Công việc xuất hiện trong kết quả tìm kiếm

## 8. Quy Trình Phát Triển

### 8.1 Thiết Lập Dự Án

1. Tạo dự án ASP.NET Core MVC
2. Cấu hình Entity Framework với PostgreSQL
3. Thiết lập Identity cho xác thực
4. Tạo các mô hình thực thể ban đầu
5. Cấu hình DbContext với các mối quan hệ
6. Tạo migration và khởi tạo cơ sở dữ liệu

### 8.2 Phương Pháp Phát Triển

Dự án tuân theo các phương pháp phát triển sau:

- Mẫu repository cho truy cập dữ liệu
- Mẫu DTO cho chuyển đổi dữ liệu
- Partial view cho các thành phần UI có thể tái sử dụng
- Các tiện ích JavaScript cho trải nghiệm UI nâng cao
- AJAX cho các thao tác bất đồng bộ
- Thiết kế responsive với Bootstrap

### 8.3 Phát Triển Frontend

Frontend được xây dựng sử dụng:

- Bootstrap cho bố cục responsive
- CSS tùy chỉnh cho styling
- JavaScript cho các tính năng tương tác
- AJAX cho việc lấy dữ liệu
- View components cho các phần tử có thể tái sử dụng

## 9. Cải Tiến Trong Tương Lai

Các cải tiến tiềm năng trong tương lai bao gồm:

1. **Hệ Thống Thông Báo**: Thông báo qua email và trong ứng dụng
2. **Gợi Ý Công Việc**: Gợi ý công việc dựa trên AI
3. **Tích Hợp Mạng Xã Hội**: Chia sẻ công việc trên mạng xã hội
4. **Phân Tích Nâng Cao**: Bảng điều khiển nhà tuyển dụng với phân tích ứng tuyển
5. **Ứng Dụng Di Động**: Ứng dụng di động native

## 10. Tài Liệu Kỹ Thuật

### 10.1 Các File Chính và Mục Đích

- **Program.cs**: Khởi động ứng dụng và cấu hình dịch vụ
- **WorkFinderContext.cs**: Context cơ sở dữ liệu với cấu hình thực thể
- **[Entity]Repository.cs**: Triển khai truy cập dữ liệu
- **[Controller].cs**: Xử lý yêu cầu và logic nghiệp vụ
- **utils.js**: Các tiện ích JavaScript bao gồm hàm debounce

### 10.2 Các Tiện Ích JavaScript

Ứng dụng bao gồm các hàm tiện ích trong utils.js:

- **debounce**: Giới hạn tần suất gọi hàm
- **getFormDataAsQueryString**: Chuyển đổi dữ liệu form thành chuỗi truy vấn URL

Các tiện ích này hỗ trợ các tính năng tương tác của ứng dụng, đặc biệt là chức năng tìm kiếm thời gian thực.

## 11. Kết Luận

WorkFinder là một ứng dụng cổng thông tin việc làm đầy đủ tính năng được xây dựng với ASP.NET Core, Entity Framework và các công nghệ web hiện đại. Nó cung cấp trải nghiệm liền mạch cho cả người tìm việc và nhà tuyển dụng, với các tính năng như tìm kiếm thời gian thực, lọc nâng cao và quy trình ứng tuyển hợp lý. Ứng dụng tuân theo các phương pháp tốt nhất trong phát triển phần mềm, bao gồm separation of concerns, mẫu repository và thiết kế responsive.
