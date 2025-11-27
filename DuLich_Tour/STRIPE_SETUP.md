# Hướng dẫn tích hợp Stripe Payment

## Bước 1: Cài đặt Stripe.NET Package

Mở **Package Manager Console** trong Visual Studio và chạy lệnh:

```
Install-Package Stripe.net -Version 43.20.0
```

Hoặc sử dụng NuGet Package Manager UI:
1. Right-click vào project `DuLich_Tour`
2. Chọn **Manage NuGet Packages**
3. Tìm kiếm `Stripe.net`
4. Cài đặt version 43.20.0

## Bước 2: Lấy Stripe API Keys

1. Đăng ký/Đăng nhập vào [Stripe Dashboard](https://dashboard.stripe.com/)
2. Chuyển sang **Test mode** (toggle ở góc trên bên phải)
3. Vào **Developers** > **API keys**
4. Bạn sẽ thấy 2 keys chính:
   - **Publishable key** (bắt đầu bằng `pk_test_...`) - Click "Reveal" hoặc "Copy" để copy toàn bộ key
   - **Secret key** (bắt đầu bằng `sk_test_...`) - Click "Reveal" hoặc "Copy" để copy toàn bộ key

**Lưu ý:** Webhook secret chỉ xuất hiện sau khi bạn tạo webhook endpoint (xem Bước 5). Hiện tại bạn chỉ cần 2 keys trên.

## Bước 3: Cấu hình Keys trong Web.config

Mở file `Web.config` và cập nhật các giá trị trong `<appSettings>`:

```xml
<!-- Copy Publishable key từ Stripe Dashboard -->
<add key="Stripe:PublishableKey" value="pk_test_YOUR_ACTUAL_PUBLISHABLE_KEY" />

<!-- Copy Secret key từ Stripe Dashboard -->
<add key="Stripe:SecretKey" value="sk_test_YOUR_ACTUAL_SECRET_KEY" />

<!-- Webhook Secret - Để trống nếu chưa thiết lập webhook -->
<add key="Stripe:WebhookSecret" value="" />
```

**Ví dụ:**
```xml
<add key="Stripe:PublishableKey" value="pk_test_51RLkzZABC123..." />
<add key="Stripe:SecretKey" value="sk_test_51RLkzZXYZ789..." />
<add key="Stripe:WebhookSecret" value="" />
```

## Bước 4: Kích hoạt Stripe Code trong PaymentController

Sau khi cài đặt package, mở file `DuLich_Tour/Controllers/PaymentController.cs` và:

1. **Uncomment dòng import:**
   ```csharp
   using Stripe;
   ```

2. **Uncomment code trong method `CreatePaymentIntent`:**
   - Tìm phần code có comment `// Code thực tế với Stripe.NET`
   - Xóa comment `/* */` và code mock

3. **Uncomment code trong method `ConfirmPayment`:**
   - Tìm phần code có comment `// Code thực tế với Stripe.NET`
   - Xóa comment `/* */` và code mock

4. **Uncomment code trong method `Webhook`:**
   - Tìm phần code có comment `// Xác thực webhook từ Stripe`
   - Xóa comment `/* */`

## Bước 5: Cấu hình Webhook (Tùy chọn - Bỏ qua nếu chưa cần)

Webhook cho phép Stripe tự động thông báo khi thanh toán thành công/thất bại. **Bạn có thể bỏ qua bước này** nếu chỉ cần test thanh toán cơ bản.

Nếu muốn sử dụng webhook:

1. Vào Stripe Dashboard > **Developers** > **Webhooks**
2. Click **Add endpoint**
3. Nhập URL: `https://yourdomain.com/Payment/Webhook` (hoặc dùng ngrok để test local)
4. Chọn events:
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
5. Sau khi tạo, click vào webhook endpoint vừa tạo
6. Copy **Signing secret** (bắt đầu bằng `whsec_...`)
7. Cập nhật vào `Web.config` với key `Stripe:WebhookSecret`

## Bước 6: Test với Thẻ Test

Stripe cung cấp các thẻ test để kiểm tra:

### Thẻ thành công:
- **Số thẻ:** `4242 4242 4242 4242`
- **Ngày hết hạn:** Bất kỳ ngày tương lai (ví dụ: 12/25)
- **CVC:** Bất kỳ 3 chữ số (ví dụ: 123)
- **ZIP:** Bất kỳ 5 chữ số (ví dụ: 12345)

### Thẻ thất bại:
- **Số thẻ:** `4000 0000 0000 0002` (Card declined)
- **Số thẻ:** `4000 0000 0000 9995` (Insufficient funds)

Xem thêm test cards tại: https://stripe.com/docs/testing

## Lưu ý quan trọng:

1. **Test Mode vs Live Mode:**
   - Hiện tại đang sử dụng **Test mode** (keys bắt đầu bằng `pk_test_` và `sk_test_`)
   - Khi deploy production, cần chuyển sang **Live mode** và cập nhật keys tương ứng

2. **Currency:**
   - Code hiện tại sử dụng currency `vnd` (VND)
   - Stripe hỗ trợ VND nhưng không có decimal, nên số tiền được nhân 100 (ví dụ: 100,000 VND = 10000000 cents)

3. **Security:**
   - **KHÔNG BAO GIỜ** commit Secret Key vào Git
   - **Web.config hiện tại đã được cấu hình với placeholder `YOUR_STRIPE_SECRET_KEY`**
   - **Bạn cần thay thế `YOUR_STRIPE_SECRET_KEY` bằng Secret Key thực tế của bạn trong file Web.config local**
   - Sử dụng Web.config transforms hoặc Azure App Settings cho production
   - Nếu đã commit Secret Key vào Git, cần xóa nó khỏi lịch sử commit trước khi push

## Troubleshooting:

- **Lỗi "Stripe configuration missing":** Kiểm tra lại Web.config đã cấu hình đúng keys chưa
- **Lỗi "Payment intent not found":** Kiểm tra paymentIntentId có đúng format không
- **Webhook không hoạt động:** Kiểm tra URL webhook và signing secret

## Tài liệu tham khảo:

- [Stripe.NET Documentation](https://github.com/stripe/stripe-dotnet)
- [Stripe Payment Intents](https://stripe.com/docs/payments/payment-intents)
- [Stripe Testing](https://stripe.com/docs/testing)

