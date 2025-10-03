-- Add service information to pets table (解决上门喂猫痛点)
-- Migration: 004_add_pet_service_info
-- Date: 2025-10-03

ALTER TABLE pets
ADD COLUMN food_location TEXT,
ADD COLUMN water_location TEXT,
ADD COLUMN litter_box_location TEXT,
ADD COLUMN cleaning_supplies_location TEXT,
ADD COLUMN needs_water_refill BOOLEAN DEFAULT FALSE,
ADD COLUMN special_instructions TEXT;

-- Add comments
COMMENT ON COLUMN pets.food_location IS '猫粮位置 (e.g., 厨房橱柜第二层)';
COMMENT ON COLUMN pets.water_location IS '水盆位置 (e.g., 客厅电视柜旁边)';
COMMENT ON COLUMN pets.litter_box_location IS '猫砂盆位置 (e.g., 卫生间角落)';
COMMENT ON COLUMN pets.cleaning_supplies_location IS '清洁用品位置 (扫把、猫屎袋等, e.g., 阳台储物柜)';
COMMENT ON COLUMN pets.needs_water_refill IS '是否需要备水';
COMMENT ON COLUMN pets.special_instructions IS '特殊说明 (e.g., 猫粮每次半碗、水要换新的)';

