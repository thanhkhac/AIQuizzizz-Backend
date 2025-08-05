FROM python:3.10-slim

# Tạo thư mục app
WORKDIR /app

# Copy file requirements và cài đặt
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy mã nguồn
COPY app.py .

# Mở port Flask
EXPOSE 1234

# Lệnh chạy Flask app
CMD ["python", "app.py"]
