-- CatCat Database Schema

-- 用户表
CREATE TABLE IF NOT EXISTS users (
    id BIGSERIAL PRIMARY KEY,
    phone VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(100),
    nick_name VARCHAR(50) NOT NULL,
    avatar VARCHAR(500),
    password_hash VARCHAR(255) NOT NULL,
    role INT NOT NULL DEFAULT 1,
    status INT NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE INDEX idx_users_phone ON users(phone);
CREATE INDEX idx_users_role ON users(role);

-- 服务人员详细信息表
CREATE TABLE IF NOT EXISTS service_providers (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id),
    real_name VARCHAR(50) NOT NULL,
    id_card VARCHAR(18) NOT NULL,
    id_card_front_url VARCHAR(500),
    id_card_back_url VARCHAR(500),
    certificate_url VARCHAR(500),
    verify_status INT NOT NULL DEFAULT 0,
    verify_remark TEXT,
    rating DECIMAL(3,2) NOT NULL DEFAULT 5.00,
    service_count INT NOT NULL DEFAULT 0,
    introduction TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE INDEX idx_service_providers_user_id ON service_providers(user_id);
CREATE INDEX idx_service_providers_verify_status ON service_providers(verify_status);

-- 宠物信息表
CREATE TABLE IF NOT EXISTS pets (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id),
    name VARCHAR(50) NOT NULL,
    type INT NOT NULL,
    breed VARCHAR(50),
    age INT NOT NULL,
    gender INT NOT NULL,
    avatar VARCHAR(500),
    character TEXT,
    dietary_habits TEXT,
    health_status TEXT,
    remarks TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE INDEX idx_pets_user_id ON pets(user_id);

-- 服务套餐表
CREATE TABLE IF NOT EXISTS service_packages (
    id BIGSERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    duration INT NOT NULL,
    icon_url VARCHAR(500),
    service_items TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    sort_order INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE INDEX idx_service_packages_is_active ON service_packages(is_active);

-- 服务订单表
CREATE TABLE IF NOT EXISTS service_orders (
    id BIGSERIAL PRIMARY KEY,
    order_no VARCHAR(32) NOT NULL UNIQUE,
    customer_id BIGINT NOT NULL REFERENCES users(id),
    service_provider_id BIGINT REFERENCES users(id),
    pet_id BIGINT NOT NULL REFERENCES pets(id),
    service_package_id BIGINT NOT NULL REFERENCES service_packages(id),
    service_date DATE NOT NULL,
    service_time TIME NOT NULL,
    address VARCHAR(200) NOT NULL,
    address_detail TEXT,
    price DECIMAL(10,2) NOT NULL,
    status INT NOT NULL DEFAULT 0,
    customer_remark TEXT,
    service_remark TEXT,
    accepted_at TIMESTAMP,
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    cancelled_at TIMESTAMP,
    cancel_reason TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE INDEX idx_service_orders_order_no ON service_orders(order_no);
CREATE INDEX idx_service_orders_customer_id ON service_orders(customer_id);
CREATE INDEX idx_service_orders_service_provider_id ON service_orders(service_provider_id);
CREATE INDEX idx_service_orders_status ON service_orders(status);
CREATE INDEX idx_service_orders_service_date ON service_orders(service_date);

-- 服务记录表（照片/视频）
CREATE TABLE IF NOT EXISTS service_records (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES service_orders(id),
    type INT NOT NULL,
    file_url VARCHAR(500) NOT NULL,
    thumbnail_url VARCHAR(500),
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_service_records_order_id ON service_records(order_id);

-- 评价表
CREATE TABLE IF NOT EXISTS reviews (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES service_orders(id),
    customer_id BIGINT NOT NULL REFERENCES users(id),
    service_provider_id BIGINT NOT NULL REFERENCES users(id),
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    content TEXT,
    photo_urls TEXT,
    reply TEXT,
    replied_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_reviews_order_id ON reviews(order_id);
CREATE INDEX idx_reviews_service_provider_id ON reviews(service_provider_id);

-- 插入初始服务套餐
INSERT INTO service_packages (name, description, price, duration, service_items, is_active, sort_order) VALUES
('基础喂养', '上门喂食、换水、清理猫砂', 30.00, 30, '["喂食","换水","清理猫砂"]', true, 1),
('标准护理', '基础喂养 + 梳毛 + 陪玩', 50.00, 60, '["喂食","换水","清理猫砂","梳毛","陪玩15分钟"]', true, 2),
('高级护理', '标准护理 + 健康检查 + 拍照反馈', 80.00, 90, '["喂食","换水","清理猫砂","梳毛","陪玩30分钟","健康检查","拍照反馈"]', true, 3);

-- 创建管理员账号 (密码: admin123)
INSERT INTO users (phone, nick_name, password_hash, role, status, created_at) VALUES
('13800138000', '系统管理员', 'hashed_password_here', 99, 1, CURRENT_TIMESTAMP);

