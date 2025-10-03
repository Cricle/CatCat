-- CatCat Database Schema
-- All-in-one initialization script
-- Date: 2025-10-03

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

-- 宠物信息表（含服务信息字段 - 解决上门喂猫痛点）
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
    -- Service Information (图片 + 描述格式)
    food_location_image TEXT,
    food_location_desc TEXT,
    water_location_image TEXT,
    water_location_desc TEXT,
    litter_box_location_image TEXT,
    litter_box_location_desc TEXT,
    cleaning_supplies_image TEXT,
    cleaning_supplies_desc TEXT,
    needs_water_refill BOOLEAN DEFAULT FALSE,
    special_instructions TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE INDEX idx_pets_user_id ON pets(user_id);

COMMENT ON COLUMN pets.food_location_image IS '猫粮位置照片';
COMMENT ON COLUMN pets.food_location_desc IS '猫粮位置描述 (e.g., 厨房橱柜第二层)';
COMMENT ON COLUMN pets.water_location_image IS '水盆位置照片';
COMMENT ON COLUMN pets.water_location_desc IS '水盆位置描述 (e.g., 客厅电视柜旁边)';
COMMENT ON COLUMN pets.litter_box_location_image IS '猫砂盆位置照片';
COMMENT ON COLUMN pets.litter_box_location_desc IS '猫砂盆位置描述 (e.g., 卫生间角落)';
COMMENT ON COLUMN pets.cleaning_supplies_image IS '清洁用品位置照片';
COMMENT ON COLUMN pets.cleaning_supplies_desc IS '清洁用品位置描述 (扫把、猫屎袋等, e.g., 阳台储物柜)';
COMMENT ON COLUMN pets.needs_water_refill IS '是否需要备水';
COMMENT ON COLUMN pets.special_instructions IS '特殊说明 (e.g., 猫粮每次半碗、水要换新的)';

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

-- 订单状态历史表
CREATE TABLE IF NOT EXISTS order_status_history (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES service_orders(id),
    from_status INT NOT NULL,
    to_status INT NOT NULL,
    remark TEXT,
    operator_id BIGINT NOT NULL,
    operator_name VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_order_status_history_order_id ON order_status_history(order_id);
CREATE INDEX idx_order_status_history_created_at ON order_status_history(created_at DESC);

-- 服务进度表
CREATE TABLE IF NOT EXISTS service_progress (
    id BIGINT PRIMARY KEY,
    order_id BIGINT NOT NULL,
    service_provider_id BIGINT NOT NULL,
    status INTEGER NOT NULL,
    description TEXT,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    address TEXT,
    image_urls TEXT,
    created_at TIMESTAMP NOT NULL,
    FOREIGN KEY (order_id) REFERENCES service_orders(id) ON DELETE CASCADE,
    FOREIGN KEY (service_provider_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_service_progress_order_id ON service_progress(order_id);
CREATE INDEX idx_service_progress_created_at ON service_progress(created_at DESC);

COMMENT ON TABLE service_progress IS 'Service progress tracking for real-time updates';
COMMENT ON COLUMN service_progress.status IS '1=OnTheWay, 2=Arrived, 3=StartService, 4=Feeding, 5=CleaningLitter, 6=Playing, 7=Grooming, 8=TakingPhotos, 9=Completed';
COMMENT ON COLUMN service_progress.image_urls IS 'JSON array of image URLs for service photos';

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

-- 支付记录表
CREATE TABLE IF NOT EXISTS payments (
    id BIGSERIAL PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES service_orders(id),
    payment_intent_id VARCHAR(255) NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'usd',
    status INT NOT NULL DEFAULT 0,
    method INT NOT NULL DEFAULT 1,
    stripe_customer_id VARCHAR(255),
    error_message TEXT,
    paid_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE INDEX idx_payments_order_id ON payments(order_id);
CREATE INDEX idx_payments_payment_intent_id ON payments(payment_intent_id);
CREATE INDEX idx_payments_status ON payments(status);

-- Refresh Token 表（JWT刷新令牌）
CREATE TABLE IF NOT EXISTS refresh_tokens (
    id BIGINT PRIMARY KEY,
    user_id BIGINT NOT NULL,
    token VARCHAR(500) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL,
    created_by_ip VARCHAR(45),
    revoked_at TIMESTAMP,
    revoked_by_ip VARCHAR(45),
    replaced_by_token VARCHAR(500),
    reason_revoked VARCHAR(200),
    CONSTRAINT fk_refresh_tokens_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);

COMMENT ON TABLE refresh_tokens IS 'Stores JWT refresh tokens for secure token rotation';

-- 插入初始服务套餐
INSERT INTO service_packages (name, description, price, duration, service_items, is_active, sort_order) VALUES
('基础喂养', '上门喂食、换水、清理猫砂', 30.00, 30, '["喂食","换水","清理猫砂"]', true, 1),
('标准护理', '基础喂养 + 梳毛 + 陪玩', 50.00, 60, '["喂食","换水","清理猫砂","梳毛","陪玩15分钟"]', true, 2),
('高级护理', '标准护理 + 健康检查 + 拍照反馈', 80.00, 90, '["喂食","换水","清理猫砂","梳毛","陪玩30分钟","健康检查","拍照反馈"]', true, 3);

-- 创建管理员账号 (密码: admin123)
INSERT INTO users (phone, nick_name, password_hash, role, status, created_at) VALUES
('13800138000', '系统管理员', 'hashed_password_here', 99, 1, CURRENT_TIMESTAMP);
