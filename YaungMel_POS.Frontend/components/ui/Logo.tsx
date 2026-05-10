import React from "react";

interface LogoProps {
  className?: string;
}

export function Logo({ className = "w-10 h-10" }: LogoProps) {
  return (
    <svg
      viewBox="0 0 100 100"
      className={className}
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      {/* Background Gradient Circle/Square */}
      <rect width="100" height="100" rx="28" fill="url(#logo-gradient)" />
      
      {/* Monitor/Screen Background */}
      <rect x="25" y="22" width="50" height="36" rx="6" fill="#1E2235" stroke="#2D324F" strokeWidth="2" />
      <rect x="29" y="26" width="42" height="28" rx="3" fill="#0F111A" />
      
      {/* Chart line on screen */}
      <path
        d="M32 46L40 38L48 42L56 34L64 38L68 28"
        stroke="#00F2FF"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        className="opacity-90"
      />
      <circle cx="68" cy="28" r="2" fill="#00F2FF" />
      
      {/* QR Code simplified */}
      <g transform="translate(56, 30) scale(0.8)">
        <rect width="12" height="12" rx="2" fill="white" />
        <rect x="2" y="2" width="3" height="3" fill="#0F111A" />
        <rect x="7" y="2" width="3" height="3" fill="#0F111A" />
        <rect x="2" y="7" width="3" height="3" fill="#0F111A" />
        <rect x="7" y="7" width="3" height="5" rx="1" fill="#0F111A" />
      </g>
      
      {/* Calculator Body */}
      <rect x="22" y="48" width="26" height="36" rx="5" fill="#2D324F" stroke="#1E2235" strokeWidth="1.5" />
      {/* Calc Screen */}
      <rect x="26" y="53" width="18" height="8" rx="1.5" fill="#0F111A" />
      <text x="28" y="59" fill="#00F2FF" style={{ fontSize: '4.5px', fontWeight: 'bold', fontFamily: 'monospace' }}>88.50</text>
      
      {/* Calc Buttons */}
      <rect x="26" y="65" width="4" height="4" rx="1" fill="#FF5C5C" />
      <rect x="33" y="65" width="4" height="4" rx="1" fill="#4E5685" />
      <rect x="40" y="65" width="4" height="4" rx="1" fill="#FFB800" />
      
      <rect x="26" y="72" width="4" height="4" rx="1" fill="#4E5685" />
      <rect x="33" y="72" width="4" height="4" rx="1" fill="#4E5685" />
      <rect x="40" y="72" width="4" height="9" rx="1.5" fill="#FFB800" />
      
      <rect x="26" y="79" width="4" height="4" rx="1" fill="#4E5685" />
      <rect x="33" y="79" width="4" height="4" rx="1" fill="#4E5685" />
      
      {/* Coins */}
      <g transform="translate(58, 68)">
        <circle cx="10" cy="10" r="11" fill="#FFD700" stroke="#E6B800" strokeWidth="1.5" />
        <text x="7" y="14" fill="#B38F00" style={{ fontSize: '11px', fontWeight: 'bold' }}>$</text>
      </g>
      <g transform="translate(68, 75)">
        <circle cx="10" cy="10" r="11" fill="#FFD700" stroke="#E6B800" strokeWidth="1.5" />
        <text x="7" y="14" fill="#B38F00" style={{ fontSize: '11px', fontWeight: 'bold' }}>$</text>
      </g>

      <defs>
        <linearGradient id="logo-gradient" x1="0" y1="0" x2="100" y2="100" gradientUnits="userSpaceOnUse">
          <stop stopColor="#7C3AED" />
          <stop offset="1" stopColor="#4F46E5" />
        </linearGradient>
      </defs>
    </svg>
  );
}
